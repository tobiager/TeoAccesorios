namespace TeoAccesorios.Desktop.UI.Reportes
{
    partial class frmReportesVentas
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
            lbl_TituloVentas = new Label();
            cmb_filtro = new ComboBox();
            lbl_filtro = new Label();
            dtpDesde = new DateTimePicker();
            dtpHasta = new DateTimePicker();
            lblFechaDesde = new Label();
            lblFechaHasta = new Label();
            rdbAscendente = new RadioButton();
            rdbDescendente = new RadioButton();
            webv_ventas = new Microsoft.Web.WebView2.WinForms.WebView2();
            btnFiltrar = new Button();
            ((System.ComponentModel.ISupportInitialize)webv_ventas).BeginInit();
            SuspendLayout();
            // 
            // lbl_TituloVentas
            // 
            lbl_TituloVentas.AutoSize = true;
            lbl_TituloVentas.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_TituloVentas.Location = new Point(424, -1);
            lbl_TituloVentas.Name = "lbl_TituloVentas";
            lbl_TituloVentas.Size = new Size(226, 32);
            lbl_TituloVentas.TabIndex = 1;
            lbl_TituloVentas.Text = "Gráficas De Ventas";
            // 
            // cmb_filtro
            // 
            cmb_filtro.FormattingEnabled = true;
            cmb_filtro.Items.AddRange(new object[] { "Temporada", "Meses", "Vendedor", "Categoria", "Subcategoria" });
            cmb_filtro.Location = new Point(99, 50);
            cmb_filtro.Name = "cmb_filtro";
            cmb_filtro.Size = new Size(136, 23);
            cmb_filtro.TabIndex = 3;
            cmb_filtro.SelectedIndexChanged += cmb_filtro_SelectedIndexChanged;
            // 
            // lbl_filtro
            // 
            lbl_filtro.AutoSize = true;
            lbl_filtro.Location = new Point(28, 53);
            lbl_filtro.Name = "lbl_filtro";
            lbl_filtro.Size = new Size(65, 15);
            lbl_filtro.TabIndex = 4;
            lbl_filtro.Text = "Ventas Por:";
            // 
            // dtpDesde
            // 
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Location = new Point(331, 48);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Size = new Size(131, 23);
            dtpDesde.TabIndex = 5;
            // 
            // dtpHasta
            // 
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Location = new Point(559, 47);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Size = new Size(104, 23);
            dtpHasta.TabIndex = 6;
            // 
            // lblFechaDesde
            // 
            lblFechaDesde.AutoSize = true;
            lblFechaDesde.Location = new Point(265, 53);
            lblFechaDesde.Name = "lblFechaDesde";
            lblFechaDesde.Size = new Size(39, 15);
            lblFechaDesde.TabIndex = 7;
            lblFechaDesde.Text = "Desde";
            // 
            // lblFechaHasta
            // 
            lblFechaHasta.AutoSize = true;
            lblFechaHasta.Location = new Point(486, 52);
            lblFechaHasta.Name = "lblFechaHasta";
            lblFechaHasta.Size = new Size(37, 15);
            lblFechaHasta.TabIndex = 8;
            lblFechaHasta.Text = "Hasta";
            // 
            // rdbAscendente
            // 
            rdbAscendente.AutoSize = true;
            rdbAscendente.Location = new Point(688, 48);
            rdbAscendente.Name = "rdbAscendente";
            rdbAscendente.Size = new Size(98, 19);
            rdbAscendente.TabIndex = 9;
            rdbAscendente.TabStop = true;
            rdbAscendente.Text = "ASCENDENTE";
            rdbAscendente.UseVisualStyleBackColor = true;
            rdbAscendente.CheckedChanged += rdbAscendente_CheckedChanged;
            // 
            // rdbDescendente
            // 
            rdbDescendente.AutoSize = true;
            rdbDescendente.Location = new Point(792, 46);
            rdbDescendente.Name = "rdbDescendente";
            rdbDescendente.Size = new Size(104, 19);
            rdbDescendente.TabIndex = 10;
            rdbDescendente.TabStop = true;
            rdbDescendente.Text = "DESCENDENTE";
            rdbDescendente.UseVisualStyleBackColor = true;
            rdbDescendente.CheckedChanged += rdbDescendente_CheckedChanged;
            // 
            // webv_ventas
            // 
            webv_ventas.AllowExternalDrop = true;
            webv_ventas.CreationProperties = null;
            webv_ventas.DefaultBackgroundColor = Color.White;
            webv_ventas.Location = new Point(12, 129);
            webv_ventas.Name = "webv_ventas";
            webv_ventas.Size = new Size(777, 348);
            webv_ventas.TabIndex = 11;
            webv_ventas.ZoomFactor = 1D;
            // 
            // btnFiltrar
            // 
            btnFiltrar.Location = new Point(368, 89);
            btnFiltrar.Name = "btnFiltrar";
            btnFiltrar.Size = new Size(75, 23);
            btnFiltrar.TabIndex = 12;
            btnFiltrar.Text = "Filtrar";
            btnFiltrar.UseVisualStyleBackColor = true;
            btnFiltrar.Click += btnFiltrar_Click;
            // 
            // frmReportesVentas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1138, 570);
            Controls.Add(btnFiltrar);
            Controls.Add(webv_ventas);
            Controls.Add(rdbDescendente);
            Controls.Add(rdbAscendente);
            Controls.Add(lblFechaHasta);
            Controls.Add(lblFechaDesde);
            Controls.Add(dtpHasta);
            Controls.Add(dtpDesde);
            Controls.Add(lbl_filtro);
            Controls.Add(cmb_filtro);
            Controls.Add(lbl_TituloVentas);
            Name = "frmReportesVentas";
            Text = "frmReportesVentas";
            ((System.ComponentModel.ISupportInitialize)webv_ventas).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webv_ventas;
        private Label lbl_TituloVentas;
        private ComboBox cmb_filtro;
        private Label lbl_filtro;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Label lblFechaDesde;
        private Label lblFechaHasta;
        private RadioButton rdbAscendente;
        private RadioButton rdbDescendente;
        private Button btnFiltrar;
    }
}