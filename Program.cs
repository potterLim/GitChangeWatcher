using System;
using System.IO;
using System.Windows.Forms;

namespace GitChangeWatcher
{
    /// <summary>
    /// ���α׷��� ������(Entry point) Ŭ�����Դϴ�.
    /// - repoPath.txt���� Git ����� ��θ� �о���̰�, 
    ///   ��ȿ�� Git ����Ұ� ���õ� ������ ������������ ���� ��� ������ �����մϴ�.
    /// - ���� ���� ��� ��, �ݱ�(X)�� ������ ���� ���� ��� ���� �ּ�ȭ�ǰ� NotifyIcon���� �����ϰ� �մϴ�.
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
                    folderDialog.Description = "Git ����Ҹ� �����ϼ���.";
                    var result = folderDialog.ShowDialog();
                    if (result != DialogResult.OK)
                    {
                        return; // ��� -> ����
                    }

                    repoPath = folderDialog.SelectedPath;
                    if (!IsValidGitRepository(repoPath))
                    {
                        MessageBox.Show("������ �������� .git ������ �����ϴ�.\n�ٽ� �������ּ���.",
                                        "�߸��� Git ����",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        repoPath = null;
                    }
                }
            }

            SaveRepoPath(repoPath);

            var mainForm = new MainForm(repoPath);

            // ���� ���� ������ �� ��, ���� ������� �ʰ� Tray�� ���⵵�� ó��
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
        /// repoPath.txt�� ����� ������ �ҷ��ɴϴ�.
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
                // ���� �б� ���� �� ����
            }
            return null;
        }

        /// <summary>
        /// ��ȿ�� Git ����� ��θ� ���Ͽ� �����մϴ�.
        /// </summary>
        private static void SaveRepoPath(string repoPath)
        {
            try
            {
                File.WriteAllText(UserDataPath.RepoPathFile, repoPath);
            }
            catch
            {
                // ���� ���� ���� �� ����
            }
        }

        /// <summary>
        /// �Է¹��� ��ΰ� ��ȿ�� Git ���������(.git ���� ���� ����) Ȯ���մϴ�.
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
