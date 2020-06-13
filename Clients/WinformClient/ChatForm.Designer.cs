namespace WinformClient {
    partial class ChatForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.quitBtn = new System.Windows.Forms.Button();
            this.subscribeBtn = new System.Windows.Forms.Button();
            this.subPanel = new System.Windows.Forms.Panel();
            this.subscribeTxtBox = new System.Windows.Forms.TextBox();
            this.subPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // quitBtn
            // 
            this.quitBtn.Location = new System.Drawing.Point(73, 323);
            this.quitBtn.Name = "quitBtn";
            this.quitBtn.Size = new System.Drawing.Size(75, 23);
            this.quitBtn.TabIndex = 0;
            this.quitBtn.Text = "Quit";
            this.quitBtn.UseVisualStyleBackColor = true;
            // 
            // subscribeBtn
            // 
            this.subscribeBtn.Location = new System.Drawing.Point(111, 23);
            this.subscribeBtn.Name = "subscribeBtn";
            this.subscribeBtn.Size = new System.Drawing.Size(75, 23);
            this.subscribeBtn.TabIndex = 0;
            this.subscribeBtn.Text = "Subscribe";
            this.subscribeBtn.UseVisualStyleBackColor = true;
            // 
            // subPanel
            // 
            this.subPanel.Controls.Add(this.subscribeTxtBox);
            this.subPanel.Controls.Add(this.subscribeBtn);
            this.subPanel.Location = new System.Drawing.Point(570, 36);
            this.subPanel.Name = "subPanel";
            this.subPanel.Size = new System.Drawing.Size(200, 100);
            this.subPanel.TabIndex = 1;
            // 
            // subscribeTxtBox
            // 
            this.subscribeTxtBox.Location = new System.Drawing.Point(5, 24);
            this.subscribeTxtBox.Name = "subscribeTxtBox";
            this.subscribeTxtBox.Size = new System.Drawing.Size(100, 23);
            this.subscribeTxtBox.TabIndex = 1;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.subPanel);
            this.Controls.Add(this.quitBtn);
            this.Name = "ChatForm";
            this.Text = "ChatClient";
            this.subPanel.ResumeLayout(false);
            this.subPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button quitBtn;
        private System.Windows.Forms.Button subscribeBtn;
        private System.Windows.Forms.Panel subPanel;
        private System.Windows.Forms.TextBox subscribeTxtBox;
    }
}

