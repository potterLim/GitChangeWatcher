using System;
using System.Windows.Forms;

namespace GitChangeWatcher
{
    /// <summary>
    /// 최근 커밋 로그를 표시해주는 폼(Form)입니다.
    /// </summary>
    public partial class CommitLogForm : Form
    {
        private readonly string mRepoName;

        /// <summary>
        /// CommitLogForm 생성자
        /// </summary>
        /// <param name="repoName">로그를 조회할 레포지토리 이름</param>
        public CommitLogForm(string repoName)
        {
            InitializeComponent();
            mRepoName = repoName;
        }

        /// <summary>
        /// 폼이 로드되면서, 해당 레포지토리의 로그 파일을 불러와 표시합니다.
        /// </summary>
        /// <param name="e">이벤트 인자</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var lines = CommitLogManager.LoadLogLines(mRepoName);
            if (lines.Length == 0)
            {
                txtLog.Text = "커밋 로그가 없습니다.";
            }
            else
            {
                txtLog.Lines = lines;
            }
        }
    }
}
