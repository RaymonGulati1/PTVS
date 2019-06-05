namespace Microsoft.PythonTools.Options {
    partial class PythonExperimentalOptionsControl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PythonExperimentalOptionsControl));
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._condaEnvironments = new System.Windows.Forms.CheckBox();
            this._condaPackageManager = new System.Windows.Forms.CheckBox();
            this._mustRestartLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this._condaEnvironments, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this._condaPackageManager, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this._mustRestartLabel, 0, 5);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // _condaEnvironments
            // 
            resources.ApplyResources(this._condaEnvironments, "_condaEnvironments");
            this._condaEnvironments.AutoEllipsis = true;
            this._condaEnvironments.Name = "_condaEnvironments";
            this._condaEnvironments.UseVisualStyleBackColor = true;
            // 
            // _condaPackageManager
            // 
            resources.ApplyResources(this._condaPackageManager, "_condaPackageManager");
            this._condaPackageManager.AutoEllipsis = true;
            this._condaPackageManager.Name = "_condaPackageManager";
            this._condaPackageManager.UseVisualStyleBackColor = true;
            // 
            // _mustRestartLabel
            // 
            resources.ApplyResources(this._mustRestartLabel, "_mustRestartLabel");
            this._mustRestartLabel.Name = "_mustRestartLabel";
            // 
            // PythonExperimentalOptionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "PythonExperimentalOptionsControl";
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label _mustRestartLabel;
        private System.Windows.Forms.CheckBox _condaEnvironments;
        private System.Windows.Forms.CheckBox _condaPackageManager;
    }
}
