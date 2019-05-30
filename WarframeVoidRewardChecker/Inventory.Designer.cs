namespace WarframeVoidRewardChecker
{
    partial class Inventory
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MainItemPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // MainItemPanel
            // 
            this.MainItemPanel.BackColor = System.Drawing.SystemColors.Control;
            this.MainItemPanel.Location = new System.Drawing.Point(10, 55);
            this.MainItemPanel.Name = "MainItemPanel";
            this.MainItemPanel.Size = new System.Drawing.Size(680, 375);
            this.MainItemPanel.TabIndex = 3;
            // 
            // Inventory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1215, 676);
            this.Controls.Add(this.MainItemPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "Inventory";
            this.Text = "Inventory";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Inventory_FormClosing);
            this.Load += new System.EventHandler(this.Inventory_Load);
            this.Resize += new System.EventHandler(this.Inventory_Resize);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label ItemName1;
        private System.Windows.Forms.CheckBox CraftedBP1;
        private System.Windows.Forms.CheckBox CraftedItem1;
        private System.Windows.Forms.Panel MainItemPanel;
    }
}