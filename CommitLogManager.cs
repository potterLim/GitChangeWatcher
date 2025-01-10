using System;
using System.IO;

namespace GitChangeWatcher
{
    /// <summary>
    /// 특정 레포지토리(commits_{repoName}.log)와 연동하여 커밋 로그를 추가/조회하는 클래스입니다.
    /// </summary>
    public static class CommitLogManager
    {
        /// <summary>
        /// 커밋이 성공적으로 이루어졌을 때, 한 줄을 로그에 기록합니다.
        /// 예) 
        /// 2025-01-10 09:01 | CommitMsg: [C89] Mix and Build | FilePath: gitrepo\src\1212.c
        /// </summary>
        /// <param name="repoName">레포지토리 이름</param>
        /// <param name="relativePath">레포지토리 기준 상대경로</param>
        /// <param name="commitMessage">커밋 메시지</param>
        public static void AppendLog(string repoName, string relativePath, string commitMessage)
        {
            try
            {
                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm} | "
                            + $"CommitMsg: {commitMessage} | "
                            + $"FilePath: {repoName}\\{relativePath}";

                string logFile = UserDataPath.GetCommitsLogFile(repoName);
                File.AppendAllLines(logFile, new[] { line });
            }
            catch
            {
                // 로그 작성 실패 시 무시
            }
        }

        /// <summary>
        /// 지정된 레포지토리에 대한 커밋 로그 파일을 한 줄씩 읽어 배열로 반환합니다.
        /// </summary>
        /// <param name="repoName">레포지토리 이름</param>
        /// <returns>커밋 로그의 각 줄이 담긴 문자열 배열</returns>
        public static string[] LoadLogLines(string repoName)
        {
            try
            {
                string logFile = UserDataPath.GetCommitsLogFile(repoName);
                if (File.Exists(logFile))
                {
                    return File.ReadAllLines(logFile);
                }
            }
            catch
            {
                // 로그 읽기 실패 시 무시
            }
            return Array.Empty<string>();
        }
    }
}
