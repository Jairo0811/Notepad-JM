using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotepadDCU
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Aqui se encuentran los codigos pertenecientes al menu ARCHIVO//


        private void NuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utiliza el metodo clear para limpiar el texto del rich textbox

            Editor.Clear();
        }

        private void AbrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se crea un objeto de openfiledialogo que nos servira para abrir archivos
            OpenFileDialog Open = new OpenFileDialog();
            System.IO.StreamReader myStreamReader = null;
            //se especifica que tipos de archivos se podran abrir y se verifica si existe
            Open.Filter = "Text [*.txt*]|*.txt|All Files [*,*]|*,*";
            Open.CheckFileExists = true;
            Open.Title = "Abrir Archivo";
            Open.ShowDialog(this);
            try
            {
                //este codigo se utiliza para que se pueda pueda mostrar la informacion del archivo que queremos abrir en el rich textbox
                Open.OpenFile();
                myStreamReader = System.IO.File.OpenText(Open.FileName);
                Editor.Text = myStreamReader.ReadToEnd();

            }
            catch (Exception) { }

        }


        private void GuardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se crea un objeto de tipo savefiledialog que nos servira para guardar el archivo
            SaveFileDialog Save = new SaveFileDialog();
            System.IO.StreamWriter myStreamWriter = null;
            //al igual que para abrir el tipo de documentos aqui se especifica en que extenciones se puede guardar el archivo
            Save.Filter = "Text (*.txt)|*.txt|HTML(*.html*)|*.html|All files(*.*)|*.*";
            Save.CheckPathExists = true;
            Save.Title = "Guardar como";
            Save.ShowDialog(this);
            try
            {
                //este codigo se utiliza para guardar el archivo de nuestro editor
                myStreamWriter = System.IO.File.AppendText(Save.FileName);
                myStreamWriter.Write(Editor.Text);
                myStreamWriter.Flush();

            }
            catch (Exception) { }

        }



        private void SalirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utiliza el metodo close para cerrar el formulario
            Close();
        }



        //Aqui se encuentran los codigos pertenecientes al menu EDICION//


        private void AdelanteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utiliza el metodo redo en le rich textbox para regresar adelante
            Editor.Redo();
        }


        private void AtrasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utiliza el metodo undo en el rich textbox para volver atras
            Editor.Undo();
        }

        private void BorrarTodoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utiliza el metodo clear para limpiar el texto del rich textbox
            Editor.Clear();
        }
   
        private void CopiarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utilza el metodo copy para copiar lo que este seleccionado en el rich textbox
            Editor.Copy();
        }

        private void CortarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utilza el metodo cut para cortar lo que este seleccionado en el rich textbox
            Editor.Cut();
        }

        private void PegarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utilza el metodo paste para pegar lo que tenemos copiado en el rich textbox
            Editor.Paste();
        }

        private void seleccionarTodoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se utilza el metodo SellecAll para seleccionar todo lo que este en el rich textbox
            Editor.SelectAll();
        }



        //Aqui se encuentran los codigos pertenecientes al menu FUENTE//

        private void FuenteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //se crea un objeto de tipo fontdialog que servira para cabiar el tipo de fuente y tamaño
            FontDialog font = new FontDialog();
            //se aplica el tipo de fuente del rich textbox al objeto que hemos creado
            font.Font = Editor.Font;
            //se hace la condicion para cambiar el tipo de fuente al presionar el boton ok
            if (font.ShowDialog() == DialogResult.OK)
            {
                Editor.Font = font.Font;
            }

        }

        private void ColorDeFuenteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se crea un objeto de tipo colordialog que servira para cabiar color de la fuente
            ColorDialog color = new ColorDialog();
            //se hace la condicion para cambiar el color de la fuente al presionar el boton ok
            if (color.ShowDialog() == DialogResult.OK)
            {
                Editor.ForeColor = color.Color;
            }
        }

        private void ColorDeFondoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //se crea un objeto de tipo colordialog que servira para cabiar color del rich textbox
            ColorDialog fondo = new ColorDialog();
            //se hace la condicion para cambiar el tipo de color del rich textbox al presionar el boton ok
            if (fondo.ShowDialog() == DialogResult.OK)
            {
                Editor.BackColor = fondo.Color;
            }
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {

        }

        private void nuevoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Editor.Clear();
        }

        private void abrirToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog Open = new OpenFileDialog();
                System.IO.StreamReader myStreamReader = null;
                //este codigo se utiliza para que se pueda pueda mostrar la informacion del archivo que queremos abrir en el rich textbox
                Open.OpenFile();
                myStreamReader = System.IO.File.OpenText(Open.FileName);
                Editor.Text = myStreamReader.ReadToEnd();

            }
            catch (Exception) { }
        }

        private void gToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog Save = new SaveFileDialog();
            System.IO.StreamWriter myStreamWriter = null;
            Save.Filter = "Text (*.txt)|*.txt|HTML(*.html*)|*.html|All files(*.*)|*.*";
            Save.CheckPathExists = true;
            Save.Title = "Guardar como";
            Save.ShowDialog(this);
            try
            {
                //este codigo se utiliza para guardar el archivo de nuestro editor
                myStreamWriter = System.IO.File.AppendText(Save.FileName);
                myStreamWriter.Write(Editor.Text);
                myStreamWriter.Flush();

            }
            catch (Exception) { }
        }

        private void salirToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void copiarToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Editor.Copy();
        }

        private void cortarToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Editor.Cut();
        }

        private void pegarToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Editor.Paste();
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog font = new FontDialog();
            //se aplica el tipo de fuente del rich textbox al objeto que hemos creado
            font.Font = Editor.Font;
            //se hace la condicion para cambiar el tipo de fuente al presionar el boton ok
            if (font.ShowDialog() == DialogResult.OK)
            {
                Editor.Font = font.Font;
            }
        }

        private void colorDeFuenteToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            //se crea un objeto de tipo colordialog que servira para cabiar color de la fuente
            ColorDialog color = new ColorDialog();
            //se hace la condicion para cambiar el color de la fuente al presionar el boton ok
            if (color.ShowDialog() == DialogResult.OK)
            {
                Editor.ForeColor = color.Color;
            }
        }

        private void colorDeFondoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            //se crea un objeto de tipo colordialog que servira para cabiar color del rich textbox
            ColorDialog fondo = new ColorDialog();
            //se hace la condicion para cambiar el tipo de color del rich textbox al presionar el boton ok
            if (fondo.ShowDialog() == DialogResult.OK)
            {
                Editor.BackColor = fondo.Color;
            }
        }
    }
}
