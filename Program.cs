using System;
using System.IO;
using System.Windows.Forms;

namespace GitChangeWatcher
{
    /// <summary>
    /// 프로그램의 진입점(Entry point) 클래스입니다.
    /// - repoPath.txt에서 Git 저장소 경로를 읽어들이고, 
    ///   유효한 Git 저장소가 선택될 때까지 폴더브라우저를 통해 경로 설정을 유도합니다.
    /// - 메인 폼을 띄운 뒤, 닫기(X)를 눌러도 실제 종료 대신 폼이 최소화되고 NotifyIcon으로 동작하게 합니다.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string repoPath = LoadRepoPath();
            while (string.IsNullOrEmpty(repoPath) || !IsValidGitRepository(repoPath))
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Git 저장소를 선택하세요.";
                    var result = folderDialog.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        return; // 취소 -> 종료
                    }

                    repoPath = folderDialog.SelectedPath;
                    if (!IsValidGitRepository(repoPath))
                    {
                        MessageBox.Show("선택한 폴더에는 .git 폴더가 없습니다.\n다시 선택해주세요.",
                                        "잘못된 Git 폴더",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        repoPath = null;
                    }
                }
            }

            SaveRepoPath(repoPath);

            var mainForm = new MainForm(repoPath);

            // 메인 폼을 닫으려 할 때, 앱이 종료되지 않고 Tray로 숨기도록 처리
            mainForm.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    mainForm.Hide();
                }
            };

            Application.Run(mainForm);
        }

        /// <summary>
        /// repoPath.txt에 저장된 정보를 불러옵니다.
        /// </summary>
        private static string LoadRepoPath()
        {
            try
            {
                if (File.Exists(UserDataPath.RepoPathFile))
                {
                    return File.ReadAllText(UserDataPath.RepoPathFile).Trim();
                }
            }
            catch
            {
                // 파일 읽기 실패 시 무시
            }
            return null;
        }

        /// <summary>
        /// 유효한 Git 저장소 경로를 파일에 저장합니다.
        /// </summary>
        private static void SaveRepoPath(string repoPath)
        {
            try
            {
                File.WriteAllText(UserDataPath.RepoPathFile, repoPath);
            }
            catch
            {
                // 파일 쓰기 실패 시 무시
            }
        }

        /// <summary>
        /// 입력받은 경로가 유효한 Git 저장소인지(.git 폴더 존재 여부) 확인합니다.
        /// </summary>
        private static bool IsValidGitRepository(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return false;
            }
            string gitFolder = Path.Combine(path, ".git");
            return Directory.Exists(gitFolder);
        }
    }
}
