using CRMDevLabs.Toolkit.Git.Models;
using CRMDevLabs.Toolkit.Telemetry;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CRMDevLabs.Toolkit.Git
{
    public class VersioningService
    {
        private readonly Action<string> m_progress;
        private readonly TelemetryWrapper m_telemetry;
        private readonly string m_repoPath;
        private readonly string m_basePath;
        private readonly string m_branch;

        public VersioningService(string solutionPath, string baseProjectPath, string branch, Action<string> reportProgress, TelemetryWrapper telemetry)
        {
            m_progress = reportProgress;
            m_telemetry = telemetry;
            m_repoPath = solutionPath;
            m_basePath = baseProjectPath;

            if (string.IsNullOrEmpty(branch))
                branch = "master";

            if (!branch.Contains("origin"))
                branch = $"origin/{branch}";

            m_branch = branch;
        }

        public SourceControlResultModel QueryLocalRepository()
        {
            SourceControlResultModel result = new SourceControlResultModel();

            try
            {
                using (var gitRepo = new Repository(m_repoPath))
                {
                    m_progress?.Invoke("------------------- START -----------------------");
                    m_progress?.Invoke($"[AZOPS] => Detecting changes in the workspace.");

                    var repoStatus = gitRepo.RetrieveStatus();
                    var totalChanges = repoStatus.Count();
                    var pendings = new List<RawChanges>();

                    m_progress?.Invoke($"[AZOPS] => Found {repoStatus.Added.Count()} added items.");
                    m_progress?.Invoke($"[AZOPS] => Found {repoStatus.Modified.Count()} modified items.");
                    m_progress?.Invoke($"[AZOPS] => Found {repoStatus.Removed.Count()} removed items.");

                    for (int i = 0; i < totalChanges; i++)
                    {
                        var item = repoStatus.ElementAt(i);

                        if (item.State != FileStatus.DeletedFromWorkdir && item.State != FileStatus.ModifiedInWorkdir && item.State != FileStatus.NewInWorkdir)
                            continue;

                        // Path normalization
                        var fixedPath = item.FilePath.Replace('/', '\\');
                        var firstSlashIndex = fixedPath.IndexOf('\\');
                        fixedPath = fixedPath.Substring(firstSlashIndex);

                        pendings.Add(new RawChanges
                        {
                            ChangeTypeName = item.State.ToString(),
                            LocalItem = string.Concat(m_basePath, fixedPath),
                            FileName = new FileInfo(item.FilePath).Name
                        });
                    }

                    result.Changes = pendings.ToArray();
                }
            }
            catch (Exception ex)
            {
                m_progress?.Invoke($"[ERROR] => {ex.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
                result.Continue = false;
            }

            return result;
        }

        public void CommitAndPush()
        {
            try
            {
                using (var gitRepo = new Repository(m_repoPath))
                {
                    var status = gitRepo.RetrieveStatus();

                    // Meaningful pending changes
                    var editPaths = status.Modified.Select(mods => mods.FilePath).ToList();
                    var addPath = status.Added.Select(mods => mods.FilePath).ToList();
                    var deletePath = status.Removed.Select(mods => mods.FilePath).ToList();

                    var allChanges = new List<string>();
                    allChanges.AddRange(editPaths);
                    allChanges.AddRange(addPath);
                    allChanges.AddRange(deletePath);

                    if (allChanges.Count > 0)
                    {
                        // Stage, Commit & Push
                        Commands.Stage(gitRepo, allChanges);
                        var agent = new Signature("Avanade Dynamics 365 Toolkit", "dynamicstoolkitsupport@avanade.com", DateTimeOffset.Now);
                        gitRepo.Commit($"CI commit by {Environment.UserName}", agent, agent);
                    }
                }
            }
            catch (Exception ex)
            {
                m_progress?.Invoke($"[ERROR] => {ex.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }
    }
}
