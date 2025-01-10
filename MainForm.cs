using System;
using System.IO;
using System.Windows.Forms;

namespace GitChangeWatcher
{
    /// <summary>
    /// 메인 폼입니다. 
    /// - 특정 Git 저장소(폴더)를 대상으로 FileSystemWatcher를 사용해 파일 변경을 감지합니다.
    /// - 감지된 파일(허용 확장자) 변경 시, 알림창을 통해 Commit/Push 과정을 수행할지 선택하게 합니다.
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

            // 폼을 표시한 직후 바로 숨기는 로직
            this.Shown += OnMainFormShown;
        }

        private void OnMainFormShown(object sender, EventArgs e)
        {
            CloseGui();
        }

        /// <summary>
        /// 최상위 저장소 경로(mRepoPath)에 대한 FileSystemWatcher를 초기화하고
        /// 특정 확장자 파일에 대한 Changed, Created, Renamed 이벤트를 감지합니다.
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
        /// 트레이 아이콘(NotifyIcon) 및 우클릭 메뉴(ContextMenuStrip)를 초기화합니다.
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

            // 메뉴 1) 열기
            mContextMenuStrip.Items.Add("열기", null, (s, e) =>
            {
                Show();
                RestoreGui();
            });

            // 메뉴 2) 최근 커밋 로그
            mContextMenuStrip.Items.Add("최근 커밋 로그", null, (s, e) =>
            {
                using (var logForm = new CommitLogForm(GetRepoName()))
                {
                    logForm.ShowDialog();
                }
            });

            // 메뉴 3) 디렉토리 변경
            mContextMenuStrip.Items.Add("디렉토리 변경", null, (s, e) =>
            {
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "Git 저장소를 선택하세요."
                };

                DialogResult result = folderDialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    return; // 취소
                }

                string newPath = folderDialog.SelectedPath;
                if (!IsValidGitRepository(newPath))
                {
                    MessageBox.Show("선택한 폴더에는 .git 폴더가 없습니다.\n다시 선택해주세요.",
                                    "잘못된 Git 폴더",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                mRepoPath = newPath;
                SaveRepoPath(newPath);

                // 감시자 재설정
                InitializeWatcher();
                // UI 숨김
                CloseGui();
                MessageBox.Show($"디렉토리가 {newPath} 로 변경되었습니다.",
                                "변경 완료",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            });

            // 메뉴 4) 종료
            mContextMenuStrip.Items.Add("종료", null, (s, e) => Application.Exit());

            mNotifyIcon.ContextMenuStrip = mContextMenuStrip;
            mNotifyIcon.DoubleClick += (s, e) =>
            {
                Show();
                RestoreGui();
            };
        }

        /// <summary>
        /// Git 저장소로 유효한지(.git 폴더가 있는지) 확인합니다.
        /// </summary>
        /// <param name="path">확인할 경로</param>
        /// <returns>유효하면 true, 아니면 false</returns>
        private bool IsValidGitRepository(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;

            string gitFolderPath = Path.Combine(path, ".git");
            return Directory.Exists(gitFolderPath);
        }

        /// <summary>
        /// 선택된 Git 저장소 경로를 파일에 저장합니다.
        /// </summary>
        /// <param name="repoPath">저장소 경로</param>
        private void SaveRepoPath(string repoPath)
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
        /// 파일이 변경(Created, Changed 등)될 때 호출되는 이벤트 핸들러입니다.
        /// </summary>
        private void onFileChanged(object sender, FileSystemEventArgs e)
        {
            // 디렉토리 자체가 생성된 경우, 그 하위 디렉토리도 감시하도록 설정
            if (Directory.Exists(e.FullPath))
            {
                AddWatcherForNewDirectory(e.FullPath);
                return;
            }

            // 실제 파일이 존재하지 않으면 무시
            if (!File.Exists(e.FullPath))
            {
                return;
            }

            // 특정 확장자만 처리
            string[] allowedExtensions = { ".cs", ".c", ".java", ".py", ".cpp" };
            string extension = Path.GetExtension(e.FullPath).ToLower();
            if (!Array.Exists(allowedExtensions, ext => ext == extension))
            {
                return;
            }

            // UI 스레드에서 UI 갱신하도록 Invoke
            Invoke(new Action(() =>
            {
                mPendingFullPath = e.FullPath;
                mPendingDisplayPath = GetDisplayPathFromRepoRoot(e.FullPath);

                lblFilePath.Text = $"파일 경로: {mPendingDisplayPath}";
                lblCommitMessage.Text = $"커밋 메시지: {GitHelper.GenerateCommitMessage(e.FullPath)}";

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
        /// 파일이 변경(Renamed)될 때 호출되는 이벤트 핸들러입니다.
        /// </summary>
        private void onFileRenamed(object sender, RenamedEventArgs e)
        {
            // Renamed도 사실상 새 파일이 생성된 것이므로
            // Created 이벤트와 동일한 로직을 사용
            var args = new FileSystemEventArgs(WatcherChangeTypes.Created,
                                               Path.GetDirectoryName(e.FullPath),
                                               e.Name);
            onFileChanged(sender, args);
        }

        /// <summary>
        /// 새로 생성된 디렉토리도 추가 감시 대상에 포함시키기 위해 사용하는 메서드입니다.
        /// </summary>
        /// <param name="directoryPath">새로 생성된 디렉토리 경로</param>
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
        /// '확인' 버튼 클릭 시 발생하는 이벤트 핸들러입니다.
        /// 실제로 git add, commit, push를 실행하고, 로그를 남깁니다.
        /// </summary>
        private void onBtnConfirmClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(mPendingFullPath))
                {
                    MessageBox.Show("파일 경로를 찾을 수 없습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string relativePath = Path.GetRelativePath(mRepoPath, mPendingFullPath);
                string commitMessage = GitHelper.GenerateCommitMessage(mPendingFullPath);

                // Git 명령 실행
                GitHelper.ExecuteGitCommands(mRepoPath, relativePath, commitMessage);

                // 로그 기록
                CommitLogManager.AppendLog(GetRepoName(), relativePath, commitMessage);

                mLastModifiedFilePath = mPendingFullPath;
                mLastCommitMessage = commitMessage;
                mLastDisplayPath = relativePath;

                MessageBox.Show(
                    $"파일: {relativePath}\n커밋 메시지: {commitMessage}\n\n변경 사항이 성공적으로 푸쉬되었습니다!",
                    "작업 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Git 명령 실행 중 오류 발생: {ex.Message}",
                                "오류",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            finally
            {
                CloseGui();
            }
        }

        /// <summary>
        /// '취소' 버튼 클릭 시 발생하는 이벤트 핸들러입니다.
        /// </summary>
        private void onBtnCancelClick(object sender, EventArgs e)
        {
            MessageBox.Show("작업이 취소되었습니다.", "작업 취소",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CloseGui();
        }

        /// <summary>
        /// 폼(UI)을 닫거나 숨길 때, 컨트롤들을 초기 상태로 되돌립니다.
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
        /// 폼을 다시 표시할 때, 최근 변경된 파일이 있으면 그 정보를 표시합니다.
        /// </summary>
        private void RestoreGui()
        {
            if (!string.IsNullOrEmpty(mLastModifiedFilePath) && !string.IsNullOrEmpty(mLastCommitMessage))
            {
                lblFilePath.Text = $"최근 파일: {mLastDisplayPath}";
                lblCommitMessage.Text = $"커밋 메시지: {mLastCommitMessage}";
            }
            else
            {
                lblFilePath.Text = "최근 변경된 파일 정보가 없습니다.";
                lblCommitMessage.Text = "현재 디렉토리를 모니터링 중입니다.";
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
        /// 전체 경로에서 레포지토리 루트 이후의 부분만 추출하여 표시용으로 씁니다.
        /// 예: C:\path\gitrepo\src\Foo.c -> gitrepo\src\Foo.c
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
        /// repoPath에서 마지막 폴더명을 추출하여 레포지토리 이름으로 사용합니다.
        /// 예: C:\path\gitrepo -> gitrepo
        /// </summary>
        private string GetRepoName()
        {
            return Path.GetFileName(mRepoPath.TrimEnd(Path.DirectorySeparatorChar));
        }
    }
}
