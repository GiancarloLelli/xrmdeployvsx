using CRMDevLabs.Toolkit.AzOps.Models;
using CRMDevLabs.Toolkit.Telemetry;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CRMDevLabs.Toolkit.AzOps
{
    public class VersioningService
    {
        private readonly Action<string> m_progress;
        private readonly TelemetryWrapper m_telemetry;
        private readonly string m_repoPath;
        private readonly string m_basePath;

        public VersioningService(string solutionPath, string baseProjectPath, Action<string> reportProgress, TelemetryWrapper telemetry)
        {
            m_progress = reportProgress;
            m_telemetry = telemetry;
            m_repoPath = solutionPath;
            m_basePath = baseProjectPath;
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
    }
}
