using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace GitChangeWatcher
{
    /// <summary>
    /// Git 명령어 실행( add → commit → push ) 및 
    /// 파일명을 통해 BOJ(백준) 문제번호 추론/문제제목 fetch 등 
    /// 다양한 헬퍼 기능을 제공하는 클래스입니다.
    /// </summary>
    public static class GitHelper
    {
        /// <summary>
        /// 단일 파일에 대해 순차적으로 git add, commit, push 명령을 실행합니다.
        /// </summary>
        /// <param name="workingDirectory">Git 저장소 경로(.git 폴더가 포함된 최상위 디렉토리)</param>
        /// <param name="fileName">추가할 파일(상대 경로)</param>
        /// <param name="commitMessage">커밋 메시지</param>
        /// <exception cref="Exception">Git 명령 실행 실패 시 예외 발생</exception>
        public static void ExecuteGitCommands(string workingDirectory, string fileName, string commitMessage)
        {
            try
            {
                RunGitCommand(workingDirectory, $"add \"{fileName}\"");
                RunGitCommand(workingDirectory, $"commit -m \"{commitMessage}\"");
                RunGitCommand(workingDirectory, "push");
            }
            catch (Exception ex)
            {
                throw new Exception($"Git 명령 실행 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 파일 확장자(.cs, .c, .cpp, .java, .py 등)에 따라 커밋 메시지 접두사를 결정하고,
        /// 파일명이 숫자이면 백준(BOJ) 사이트에서 문제 제목을 가져와 메시지에 반영합니다.
        /// </summary>
        /// <param name="filePath">실제 파일의 전체 경로</param>
        /// <returns>커밋 메시지 예: "[C언어] A+B"</returns>
        public static string GenerateCommitMessage(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            string prefix = extension switch
            {
                ".cs" => "[C#]",
                ".c" => "[C언어]",
                ".cpp" => "[C++]",
                ".java" => "[Java]",
                ".py" => "[Python]",
                _ => "[Unknown]"
            };

            // 파일명이 숫자인 경우, 백준(BOJ)에서 문제 제목을 가져와 사용
            // 숫자가 아니라면 "알 수 없는 문제"로 처리
            string problemTitle = FetchProblemTitle(filePath) ?? "알 수 없는 문제";
            return $"{prefix} {problemTitle}";
        }

        /// <summary>
        /// 파일명(확장자 제외) 부분이 숫자라면, 해당 숫자를 백준 문제 번호로 간주하고 
        /// 문제 제목을 파싱하여 반환합니다.
        /// </summary>
        /// <param name="filePath">실제 파일 경로</param>
        /// <returns>문제 제목 (예: "A+B"), 실패 시 null</returns>
        private static string FetchProblemTitle(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (!int.TryParse(fileName, out int problemNumber))
            {
                return null; // 파일명이 숫자가 아니면 백준 문제로 간주 불가
            }

            string url = $"https://www.acmicpc.net/problem/{problemNumber}";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    string pageContent = response.Content.ReadAsStringAsync().Result;
                    // 예: <title>1000번: A+B</title>
                    var match = Regex.Match(pageContent, @"<title>\d+번: (.+?)</title>");
                    if (match.Success)
                    {
                        return match.Groups[1].Value.Trim();
                    }
                }
            }
            catch
            {
                // 문제 제목 가져오기 실패 시 null
            }
            return null;
        }

        /// <summary>
        /// git 명령을 외부 프로세스로 실행시키고, 실패 시 예외를 발생시킵니다.
        /// </summary>
        /// <param name="workingDirectory">Git 저장소의 루트 디렉토리</param>
        /// <param name="command">실행할 Git 커맨드 예) add "fileName", commit -m "메시지" 등</param>
        private static void RunGitCommand(string workingDirectory, string command)
        {
            var processInfo = new ProcessStartInfo("git", command)
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception(process.StandardError.ReadToEnd());
            }
        }
    }
}
