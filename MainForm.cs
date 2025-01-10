using System;
using System.IO;
using System.Windows.Forms;

namespace GitChangeWatcher
{
    /// <summary>
    /// ���� ���Դϴ�. 
    /// - Ư�� Git �����(����)�� ������� FileSystemWatcher�� ����� ���� ������ �����մϴ�.
    /// - ������ ����(��� Ȯ����) ���� ��, �˸�â�� ���� Commit/Push ������ �������� �����ϰ� �մϴ�.
    /// </summary>
    public partial class MainForm : Form
    {
        private string mRepoPath;
        private FileSystemWatcher mWatcher;
        private NotifyIcon mNotifyIcon;
        private ContextMenuStrip mContextMenuStrip;

        private string mLastModifiedFilePath = null;
        private string mLastCommitMessage = null;
        private string mLastDisplayPath = null;

        private string mPendingFullPath = null;
        private string mPendingDisplayPath = null;

        public MainForm(string repoPath)
        {
            InitializeComponent();
            mRepoPath = repoPath;
            InitializeWatcher();
            InitializeNotifyIcon();

            // ���� ǥ���� ���� �ٷ� ����� ����
            this.Shown += OnMainFormShown;
        }

        private void OnMainFormShown(object sender, EventArgs e)
        {
            CloseGui();
        }

        /// <summary>
        /// �ֻ��� ����� ���(mRepoPath)�� ���� FileSystemWatcher�� �ʱ�ȭ�ϰ�
        /// Ư�� Ȯ���� ���Ͽ� ���� Changed, Created, Renamed �̺�Ʈ�� �����մϴ�.
        /// </summary>
        private void InitializeWatcher()
        {
            if (mWatcher != null)
            {
                mWatcher.Dispose();
            }

            mWatcher = new FileSystemWatcher
            {
                Path = mRepoPath,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            };

            mWatcher.Created += onFileChanged;
            mWatcher.Changed += onFileChanged;
            mWatcher.Renamed += onFileRenamed;
            mWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Ʈ���� ������(NotifyIcon) �� ��Ŭ�� �޴�(ContextMenuStrip)�� �ʱ�ȭ�մϴ�.
        /// </summary>
        private void InitializeNotifyIcon()
        {
            mNotifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "GitChangeWatcher"
            };

            mContextMenuStrip = new ContextMenuStrip();

            // �޴� 1) ����
            mContextMenuStrip.Items.Add("����", null, (s, e) =>
            {
                Show();
                RestoreGui();
            });

            // �޴� 2) �ֱ� Ŀ�� �α�
            mContextMenuStrip.Items.Add("�ֱ� Ŀ�� �α�", null, (s, e) =>
            {
                using (var logForm = new CommitLogForm(GetRepoName()))
                {
                    logForm.ShowDialog();
                }
            });

            // �޴� 3) ���丮 ����
            mContextMenuStrip.Items.Add("���丮 ����", null, (s, e) =>
            {
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "Git ����Ҹ� �����ϼ���."
                };

                DialogResult result = folderDialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return; // ���
                }

                string newPath = folderDialog.SelectedPath;
                if (!IsValidGitRepository(newPath))
                {
                    MessageBox.Show("������ �������� .git ������ �����ϴ�.\n�ٽ� �������ּ���.",
                                    "�߸��� Git ����",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                mRepoPath = newPath;
                SaveRepoPath(newPath);

                // ������ �缳��
                InitializeWatcher();
                // UI ����
                CloseGui();
                MessageBox.Show($"���丮�� {newPath} �� ����Ǿ����ϴ�.",
                                "���� �Ϸ�",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            });

            // �޴� 4) ����
            mContextMenuStrip.Items.Add("����", null, (s, e) => Application.Exit());

            mNotifyIcon.ContextMenuStrip = mContextMenuStrip;
            mNotifyIcon.DoubleClick += (s, e) =>
            {
                Show();
                RestoreGui();
            };
        }

        /// <summary>
        /// Git ����ҷ� ��ȿ����(.git ������ �ִ���) Ȯ���մϴ�.
        /// </summary>
        /// <param name="path">Ȯ���� ���</param>
        /// <returns>��ȿ�ϸ� true, �ƴϸ� false</returns>
        private bool IsValidGitRepository(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            string gitFolderPath = Path.Combine(path, ".git");
            return Directory.Exists(gitFolderPath);
        }

        /// <summary>
        /// ���õ� Git ����� ��θ� ���Ͽ� �����մϴ�.
        /// </summary>
        /// <param name="repoPath">����� ���</param>
        private void SaveRepoPath(string repoPath)
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
        /// ������ ����(Created, Changed ��)�� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
        /// </summary>
        private void onFileChanged(object sender, FileSystemEventArgs e)
        {
            // ���丮 ��ü�� ������ ���, �� ���� ���丮�� �����ϵ��� ����
            if (Directory.Exists(e.FullPath))
            {
                AddWatcherForNewDirectory(e.FullPath);
                return;
            }

            // ���� ������ �������� ������ ����
            if (!File.Exists(e.FullPath))
            {
                return;
            }

            // Ư�� Ȯ���ڸ� ó��
            string[] allowedExtensions = { ".cs", ".c", ".java", ".py", ".cpp" };
            string extension = Path.GetExtension(e.FullPath).ToLower();
            if (!Array.Exists(allowedExtensions, ext => ext == extension))
            {
                return;
            }

            // UI �����忡�� UI �����ϵ��� Invoke
            Invoke(new Action(() =>
            {
                mPendingFullPath = e.FullPath;
                mPendingDisplayPath = GetDisplayPathFromRepoRoot(e.FullPath);

                lblFilePath.Text = $"���� ���: {mPendingDisplayPath}";
                lblCommitMessage.Text = $"Ŀ�� �޽���: {GitHelper.GenerateCommitMessage(e.FullPath)}";

                lblFilePath.Visible = true;
                lblCommitMessage.Visible = true;
                btnConfirm.Visible = true;
                btnCancel.Visible = true;

                Show();
                TopMost = true;
                BringToFront();
            }));
        }

        /// <summary>
        /// ������ ����(Renamed)�� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
        /// </summary>
        private void onFileRenamed(object sender, RenamedEventArgs e)
        {
            // Renamed�� ��ǻ� �� ������ ������ ���̹Ƿ�
            // Created �̺�Ʈ�� ������ ������ ���
            var args = new FileSystemEventArgs(WatcherChangeTypes.Created,
                                               Path.GetDirectoryName(e.FullPath),
                                               e.Name);
            onFileChanged(sender, args);
        }

        /// <summary>
        /// ���� ������ ���丮�� �߰� ���� ��� ���Խ�Ű�� ���� ����ϴ� �޼����Դϴ�.
        /// </summary>
        /// <param name="directoryPath">���� ������ ���丮 ���</param>
        private void AddWatcherForNewDirectory(string directoryPath)
        {
            var subWatcher = new FileSystemWatcher
            {
                Path = directoryPath,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true
            };

            subWatcher.Created += onFileChanged;
            subWatcher.Changed += onFileChanged;
            subWatcher.Renamed += onFileRenamed;
            subWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// 'Ȯ��' ��ư Ŭ�� �� �߻��ϴ� �̺�Ʈ �ڵ鷯�Դϴ�.
        /// ������ git add, commit, push�� �����ϰ�, �α׸� ����ϴ�.
        /// </summary>
        private void onBtnConfirmClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(mPendingFullPath))
                {
                    MessageBox.Show("���� ��θ� ã�� �� �����ϴ�.", "����",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string relativePath = Path.GetRelativePath(mRepoPath, mPendingFullPath);
                string commitMessage = GitHelper.GenerateCommitMessage(mPendingFullPath);

                // Git ��� ����
                GitHelper.ExecuteGitCommands(mRepoPath, relativePath, commitMessage);

                // �α� ���
                CommitLogManager.AppendLog(GetRepoName(), relativePath, commitMessage);

                mLastModifiedFilePath = mPendingFullPath;
                mLastCommitMessage = commitMessage;
                mLastDisplayPath = relativePath;

                MessageBox.Show(
                    $"����: {relativePath}\nĿ�� �޽���: {commitMessage}\n\n���� ������ ���������� Ǫ���Ǿ����ϴ�!",
                    "�۾� �Ϸ�",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Git ��� ���� �� ���� �߻�: {ex.Message}",
                                "����",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            finally
            {
                CloseGui();
            }
        }

        /// <summary>
        /// '���' ��ư Ŭ�� �� �߻��ϴ� �̺�Ʈ �ڵ鷯�Դϴ�.
        /// </summary>
        private void onBtnCancelClick(object sender, EventArgs e)
        {
            MessageBox.Show("�۾��� ��ҵǾ����ϴ�.", "�۾� ���",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CloseGui();
        }

        /// <summary>
        /// ��(UI)�� �ݰų� ���� ��, ��Ʈ�ѵ��� �ʱ� ���·� �ǵ����ϴ�.
        /// </summary>
        private void CloseGui()
        {
            lblFilePath.Text = string.Empty;
            lblCommitMessage.Text = string.Empty;

            lblFilePath.Visible = false;
            lblCommitMessage.Visible = false;
            btnConfirm.Visible = false;
            btnCancel.Visible = false;

            mPendingFullPath = null;
            mPendingDisplayPath = null;

            Hide();
        }

        /// <summary>
        /// ���� �ٽ� ǥ���� ��, �ֱ� ����� ������ ������ �� ������ ǥ���մϴ�.
        /// </summary>
        private void RestoreGui()
        {
            if (!string.IsNullOrEmpty(mLastModifiedFilePath) && !string.IsNullOrEmpty(mLastCommitMessage))
            {
                lblFilePath.Text = $"�ֱ� ����: {mLastDisplayPath}";
                lblCommitMessage.Text = $"Ŀ�� �޽���: {mLastCommitMessage}";
            }
            else
            {
                lblFilePath.Text = "�ֱ� ����� ���� ������ �����ϴ�.";
                lblCommitMessage.Text = "���� ���丮�� ����͸� ���Դϴ�.";
                lblFilePath.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                lblCommitMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            }

            lblFilePath.Visible = true;
            lblCommitMessage.Visible = true;
            btnConfirm.Visible = false;
            btnCancel.Visible = false;

            TopMost = true;
            BringToFront();
        }

        /// <summary>
        /// ��ü ��ο��� �������丮 ��Ʈ ������ �κи� �����Ͽ� ǥ�ÿ����� ���ϴ�.
        /// ��: C:\path\gitrepo\src\Foo.c -> gitrepo\src\Foo.c
        /// </summary>
        private string GetDisplayPathFromRepoRoot(string fullPath)
        {
            string repoRootName = Path.GetFileName(mRepoPath.TrimEnd(Path.DirectorySeparatorChar));
            if (string.IsNullOrEmpty(repoRootName))
            {
                return fullPath;
            }
            string lowerFull = fullPath.ToLower();
            int index = lowerFull.IndexOf(repoRootName.ToLower());
            if (index < 0) return fullPath;
            return fullPath.Substring(index);
        }

        /// <summary>
        /// repoPath���� ������ �������� �����Ͽ� �������丮 �̸����� ����մϴ�.
        /// ��: C:\path\gitrepo -> gitrepo
        /// </summary>
        private string GetRepoName()
        {
            return Path.GetFileName(mRepoPath.TrimEnd(Path.DirectorySeparatorChar));
        }
    }
}
