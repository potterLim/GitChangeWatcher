namespace GitChangeWatcher
{
    // 디자이너 관련 partial 클래스: 기본 폼 구성(레이아웃 등)
    public partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblFilePath;
        private Label lblCommitMessage;
        private Button btnConfirm;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblFilePath = new System.Windows.Forms.Label();
            this.lblCommitMessage = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = false;
            this.lblFilePath.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblFilePath.Location = new System.Drawing.Point(20, 25);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(520, 50);
            this.lblFilePath.TabIndex = 0;
            this.lblFilePath.Text = "파일 경로:";
            this.lblFilePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblFilePath.Visible = false;
            // 
            // lblCommitMessage
            // 
            this.lblCommitMessage.AutoSize = false;
            this.lblCommitMessage.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCommitMessage.Location = new System.Drawing.Point(20, 75);
            this.lblCommitMessage.Name = "lblCommitMessage";
            this.lblCommitMessage.Size = new System.Drawing.Size(520, 50);
            this.lblCommitMessage.TabIndex = 1;
            this.lblCommitMessage.Text = "커밋 메시지:";
            this.lblCommitMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCommitMessage.Visible = false;
            // 
            // btnConfirm
            // 
            this.btnConfirm.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnConfirm.Location = new System.Drawing.Point(160, 140);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(110, 45);
            this.btnConfirm.TabIndex = 2;
            this.btnConfirm.Text = "확인";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Visible = false;
            this.btnConfirm.Click += new System.EventHandler(this.onBtnConfirmClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCancel.Location = new System.Drawing.Point(290, 140);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 45);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "취소";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            this.btnCancel.Click += new System.EventHandler(this.onBtnCancelClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 220);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.lblCommitMessage);
            this.Controls.Add(this.lblFilePath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "파일 변경 감지됨";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
