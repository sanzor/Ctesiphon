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
            this.components = new System.ComponentModel.Container();
            this.quitBtn = new System.Windows.Forms.Button();
            this.subscribeBtn = new System.Windows.Forms.Button();
            this.subPanel = new System.Windows.Forms.Panel();
            this.subscribeTb = new System.Windows.Forms.TextBox();
            this.connectBtn = new System.Windows.Forms.Button();
            this.controlPn = new System.Windows.Forms.Panel();
            this.msgTb = new System.Windows.Forms.TextBox();
            this.disconnectBtn = new System.Windows.Forms.Button();
            this.connectTb = new System.Windows.Forms.TextBox();
            this.msgTbProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.subPanel.SuspendLayout();
            this.controlPn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.msgTbProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // quitBtn
            // 
            this.quitBtn.Location = new System.Drawing.Point(54, 380);
            this.quitBtn.Name = "quitBtn";
            this.quitBtn.Size = new System.Drawing.Size(116, 40);
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
            this.subPanel.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.subPanel.Controls.Add(this.subscribeTb);
            this.subPanel.Controls.Add(this.subscribeBtn);
            this.subPanel.Location = new System.Drawing.Point(570, 36);
            this.subPanel.Name = "subPanel";
            this.subPanel.Size = new System.Drawing.Size(200, 100);
            this.subPanel.TabIndex = 1;
            // 
            // subscribeTb
            // 
            this.subscribeTb.Location = new System.Drawing.Point(5, 24);
            this.subscribeTb.Name = "subscribeTb";
            this.subscribeTb.Size = new System.Drawing.Size(100, 23);
            this.subscribeTb.TabIndex = 1;
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(50, 12);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(75, 23);
            this.connectBtn.TabIndex = 2;
            this.connectBtn.Text = "Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            // 
            // controlPn
            // 
            this.controlPn.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.controlPn.Controls.Add(this.msgTb);
            this.controlPn.Controls.Add(this.disconnectBtn);
            this.controlPn.Controls.Add(this.connectTb);
            this.controlPn.Controls.Add(this.connectBtn);
            this.controlPn.Location = new System.Drawing.Point(54, 36);
            this.controlPn.Name = "controlPn";
            this.controlPn.Size = new System.Drawing.Size(341, 189);
            this.controlPn.TabIndex = 3;
            // 
            // msgTb
            // 
            this.msgTb.Enabled = false;
            this.msgTb.Location = new System.Drawing.Point(131, 41);
            this.msgTb.Multiline = true;
            this.msgTb.Name = "msgTb";
            this.msgTb.Size = new System.Drawing.Size(183, 89);
            this.msgTb.TabIndex = 4;
            // 
            // disconnectBtn
            // 
            this.disconnectBtn.Enabled = false;
            this.disconnectBtn.Location = new System.Drawing.Point(50, 12);
            this.disconnectBtn.Name = "disconnectBtn";
            this.disconnectBtn.Size = new System.Drawing.Size(75, 23);
            this.disconnectBtn.TabIndex = 2;
            this.disconnectBtn.Text = "Disconnect";
            this.disconnectBtn.UseVisualStyleBackColor = true;
            this.disconnectBtn.Visible = false;
            // 
            // connectTb
            // 
            this.connectTb.Location = new System.Drawing.Point(131, 12);
            this.connectTb.Name = "connectTb";
            this.connectTb.Size = new System.Drawing.Size(183, 23);
            this.connectTb.TabIndex = 3;
            this.connectTb.Validating += new System.ComponentModel.CancelEventHandler(this.connectTb_Validating);
            // 
            // msgTbProvider
            // 
            this.msgTbProvider.ContainerControl = this;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.controlPn);
            this.Controls.Add(this.subPanel);
            this.Controls.Add(this.quitBtn);
            this.Name = "ChatForm";
            this.Text = "ChatClient";
            this.subPanel.ResumeLayout(false);
            this.subPanel.PerformLayout();
            this.controlPn.ResumeLayout(false);
            this.controlPn.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.msgTbProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button quitBtn;
        private System.Windows.Forms.Button subscribeBtn;
        private System.Windows.Forms.Panel subPanel;
        private System.Windows.Forms.TextBox subscribeTb;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Panel controlPn;
        private System.Windows.Forms.TextBox connectTb;
        private System.Windows.Forms.Button disconnectBtn;
        private System.Windows.Forms.TextBox msgTb;
        private System.Windows.Forms.ErrorProvider msgTbProvider;
    }
}

