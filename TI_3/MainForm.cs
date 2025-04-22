using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Numerics;

namespace TI_3;

public partial class MainForm : Form
{
    BigInteger IntegerR { get; set; }
    BigInteger IntegerFunctionR { get; set; }
    BigInteger IntegerE { get; set; }
    BigInteger IntegerD { get; set; }

    byte[] OpenedPlainFileBytes { get; set; }
    
    byte[] OpenedCipherFileBytes { get; set; }

    

    ushort[] CipherResult { get; set; }

    byte[] DecipherResult { get; set; }

    
    
    public MainForm()
    {
        InitializeComponent();
    }

    void ButtonR_Click(object sender, EventArgs e)
    {
        TextBoxP.Text = string.Join("", TextBoxP.Text.Where(char.IsDigit));
        TextBoxQ.Text = string.Join("", TextBoxQ.Text.Where(char.IsDigit));
        TextBoxD.Text = string.Join("", TextBoxD.Text.Where(char.IsDigit));

        if (TextBoxP.Text.Length == 0)
        {
			MessageBox.Show("Недопустимое значение: P не должен быть нулевым!", "Проблема");
			return;
        }
        
        if (TextBoxQ.Text.Length == 0)
        {
			MessageBox.Show("Недопустимое значение: Q не должен быть нулевым!", "Проблема");
			return;
        }

        BigInteger IntegerP = 0;
        BigInteger IntegerQ = 0;
        try
        {

            IntegerP = BigInteger.Parse(TextBoxP.Text);
            if (!RSA.IsPrime(IntegerP))
            {
				MessageBox.Show("Недопустимое значение: P не является простым!", "Проблема");
				return;
            }

            IntegerQ = BigInteger.Parse(TextBoxQ.Text);
            if (!RSA.IsPrime(IntegerQ))
            {
				MessageBox.Show("Недопустимое значение: Q не является простым!", "Проблема");
				return;
            }
        }
        catch
        {
			MessageBox.Show("Пожалуйста, убедитесь, что числа находятся в допустимом диапазоне.");
			return;
        }

        IntegerR = IntegerQ * IntegerP;
		if (IntegerR < 256 || IntegerR > ushort.MaxValue)
		{
			MessageBox.Show($"Значение P * Q должно быть не менее 256 и не превышать {ushort.MaxValue}.", "Проблема");
			return;
		}

		TextBoxR.Text = IntegerR.ToString();
        IntegerFunctionR = RSA.EulerPhi(IntegerR);
        TextBoxEuler.Text = IntegerFunctionR.ToString();

        if (TextBoxD.Text.Length == 0)
        {
			MessageBox.Show("Значение закрытой константы Кс не должно быть пустым.", "Проблема");
			return;
        }
        
        IntegerD = BigInteger.Parse(TextBoxD.Text);
        if (IntegerD <= 1 || IntegerD >= IntegerFunctionR)
        {
			MessageBox.Show("Значение Кс должно быть в пределах от 1 до φ(R) - 1.", "Проблема");
			return;
        }
        
        BigInteger gcd = RSA.FindGcd(IntegerD, IntegerFunctionR);
        if (gcd != 1)
        {
			MessageBox.Show("Ваша открытая константа Ko не является взаимно простой с функцией Эйлера!", "Предупреждение");
			return;
        }

        var extendedEuclidResult = RSA.ExtendedEuclidean(IntegerFunctionR, IntegerD);
        
        IntegerE = extendedEuclidResult.y;

        TextBoxE.Text = IntegerE.ToString();

        ResultButton.Enabled = true;
    }

    void ClearStrip_Click(object sender, EventArgs e)
    {
        TextBoxR.Clear();
        TextBoxEuler.Clear();
        TextBoxE.Clear();
        PlainText.Clear();
        CipherText.Clear();
        ResultButton.Enabled = false;
    }

    void RadioButtonCipher_CheckedChanged(object sender, EventArgs e)
    {
        button3.Enabled = false;
        button1.Enabled = true;
        button2.Enabled = true;
        button4.Enabled = false;
        ResultButton.Text = "Зашифровать/Дешифровать";
        CipherText.Clear();
    }

    void RadioButtonDecipher_CheckedChanged(object sender, EventArgs e)
    {
        button1.Enabled = false;
        button3.Enabled = true;
        button4.Enabled = true;
        button2.Enabled = false;
		ResultButton.Text = "Зашифровать/Дешифровать";
		PlainText.Clear();
    }

    void ResultButton_Click(object sender, EventArgs e)
    {
        if (RadioButtonCipher.Checked)
        {
            if (PlainText.Text.Length == 0)
            {
				MessageBox.Show("Исходный текст не должен быть пустым. Пожалуйста, откройте файл.", "Проблема");

				return;
            }

            CipherResult = new ushort[OpenedPlainFileBytes.Length];
            for (int i = 0; i < CipherResult.Length; i++)
            {
                CipherResult[i] = OpenedPlainFileBytes[i];
            }
            
            for (int i = 0; i < CipherResult.Length; i++)
            {
                CipherResult[i] = (ushort)RSA.QuickPowerMod(CipherResult[i], IntegerE, IntegerR);
            }

            CipherText.Text = string.Join(" ", CipherResult);
        }

        if (RadioButtonDecipher.Checked)
        {
            if (CipherText.Text.Length == 0)
            {
				MessageBox.Show("Зашифрованный текст отсутствует. Пожалуйста, откройте файл для продолжения.", "Проблема");

				return;
            }

            ushort[] tempShort = new ushort[CipherResult.Length];
            
            for (int i = 0; i < tempShort.Length; i++)
            {
                tempShort[i] = (ushort)RSA.QuickPowerMod(CipherResult[i], IntegerD, IntegerR);
            }

            DecipherResult = new byte[tempShort.Length];
            for (var index = 0; index < tempShort.Length; index++)
            {
                var item = tempShort[index];
                var bytes = BitConverter.GetBytes(item);
                if (!BitConverter.IsLittleEndian) 
                    Array.Reverse(bytes);
                DecipherResult[index] = bytes[0];

            }

            PlainText.Text = string.Join(" ", tempShort);
        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        if (OpenFileDialog.ShowDialog() != DialogResult.Cancel)
        {
            OpenedPlainFileBytes = File.ReadAllBytes(OpenFileDialog.FileName);
            PlainText.Text = string.Join(" ", OpenedPlainFileBytes);
        }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        if (CipherText.Text.Length == 0)
        {
            MessageBox.Show("Пусто!", "Проблема");
            return;
        }
        if (SaveFileDialog.ShowDialog() != DialogResult.Cancel)
        {
            using FileStream fileStream = new FileStream(SaveFileDialog.FileName, FileMode.Create);
            foreach (var item in CipherResult)
            {
                byte[] bytes = BitConverter.GetBytes(item);
                fileStream.Write(bytes, 0, bytes.Length);
            }
        }
    }

    private void button3_Click(object sender, EventArgs e)
    {
        if (OpenFileDialog.ShowDialog() != DialogResult.Cancel)
        {
            OpenedCipherFileBytes = File.ReadAllBytes(OpenFileDialog.FileName);

            if (OpenedCipherFileBytes.Length % 2 != 0)
            {
                OpenedCipherFileBytes = OpenedCipherFileBytes.Append((byte)0).ToArray();
            }

            CipherResult = new ushort[OpenedCipherFileBytes.Length / 2];

            for (int i = 0; i < OpenedCipherFileBytes.Length; i += 2)
            {
                byte[] bytes = [OpenedCipherFileBytes[i], OpenedCipherFileBytes[i + 1]];
                ushort combinedShort = BitConverter.ToUInt16(bytes, 0);
                CipherResult[i / 2] = combinedShort;
            }

            CipherText.Text = string.Join(" ", CipherResult);
        }
    }

    private void button4_Click(object sender, EventArgs e)
    {
        if (PlainText.Text.Length == 0)
        {
			MessageBox.Show("Пусто!", "Проблема");
			return;
        }

        if (SaveFileDialog.ShowDialog() != DialogResult.Cancel)
        {
            File.WriteAllBytes(SaveFileDialog.FileName, DecipherResult);
        }
    }

    private void button5_Click(object sender, EventArgs e)
    {
        TextBoxR.Clear();
        TextBoxEuler.Clear();
        TextBoxE.Clear();
        PlainText.Clear();
        CipherText.Clear();
        ResultButton.Enabled = false;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {

    }

	private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{

	}

	private void LabelD_Click(object sender, EventArgs e)
	{

	}

	private void TextBoxE_TextChanged(object sender, EventArgs e)
	{

	}
}