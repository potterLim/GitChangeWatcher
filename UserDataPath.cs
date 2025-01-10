using System;
using System.IO;

namespace GitChangeWatcher
{
    /// <summary>
    /// 사용자 데이터 폴더 경로 관리 클래스입니다. 
    /// - %AppData%\GitChangeWatcher 폴더를 생성 및 관리합니다.
    /// - repoPath.txt, commits_{repoName}.log 등의 파일 경로를 제공합니다.
    /// </summary>
    public static class UserDataPath
    {
        private static readonly string AppFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "GitChangeWatcher");

        /// <summary>
        /// 저장소 경로(repoPath)를 저장하는 파일 경로를 반환합니다.
        /// </summary>
        public static string RepoPathFile => Path.Combine(AppFolder, "repoPath.txt");

        /// <summary>
        /// 특정 레포지토리명을 인자로 받아 해당 레포지토리 커밋 로그(commits_{repoName}.log) 파일 경로를 반환합니다.
        /// </summary>
        /// <param name="repoName">레포지토리명</param>
        /// <returns>commits_{repoName}.log 파일 경로</returns>
        public static string GetCommitsLogFile(string repoName)
        {
            return Path.Combine(AppFolder, $"commits_{repoName}.log");
        }

        static UserDataPath()
        {
            try
            {
                if (!Directory.Exists(AppFolder))
                {
                    Directory.CreateDirectory(AppFolder);
                }
            }
            catch
            {
                // 폴더 생성 실패 시 무시
            }
        }
    }
}
