using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;


public class ScoreTracker : Form
{

	[DllImport("kernel32.dll")]
	static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	const int SW_HIDE = 0;
	const int SW_SHOW = 5;
	
	private bool closing = false;

	public static FileReader config;
	public static FileReader pbEasy;
	public static FileReader pbHard;
	public static FileReader individualLevels;

	//  Declare and initilize UI elements
	//private LogBox chatLog = new LogBox(); //  LogBox is my own class that has a method for adding messages and built in thread safety
	private TextBox inputBox = new TextBox();
	private Panel totals = new Panel();
	private Panel levels = new Panel();
	public static Label topScore = new Label();
	public static Label sobScore = new Label();
	public ScoreInput sInput = new ScoreInput();

	//private Button sendButton = new Button();
	//private TabControl rooms = new TabControl();

	//  Declare colors
	public static Color text_color;
	public static Color background_color_highlighted;
	public static Color background_color = ColorTranslator.FromHtml("#082045");
	public static Color text_color_highlighted;
	public static Color text_color_ahead;
	public static Color text_color_behind;
	public static Color text_color_best;
	public static Color text_color_total;

	//private string[] pb_run;
	//private string[] ils;
	//private string peanut_butter;
	//private string individual_levels;

	//private List<Score> scores = new List<Score>();

	public ScoreTracker()
	{

		text_color = ColorTranslator.FromHtml(config["text_color"]);
		background_color_highlighted = ColorTranslator.FromHtml(config["background_color_highlighted"]);
		background_color = ColorTranslator.FromHtml(config["background_color"]);
		text_color_highlighted = ColorTranslator.FromHtml(config["text_color_highlighted"]);
		text_color_ahead = ColorTranslator.FromHtml(config["text_color_ahead"]);
		text_color_behind = ColorTranslator.FromHtml(config["text_color_behind"]);
		text_color_best = ColorTranslator.FromHtml(config["text_color_best"]);
		text_color_total = ColorTranslator.FromHtml(config["text_color_total"]);
		//this.peanut_butter = peanut_butter;
		//this.individual_levels = individual_levels;

		Font = new Font(config["font"], Int32.Parse(config["font_size"]), FontStyle.Bold);
		Text = "Star Fox 64 Score Tracker";
		sInput.Text = "Input";
		sInput.FormClosing += new FormClosingEventHandler(ConfirmClose);
		FormClosing += new FormClosingEventHandler(ConfirmClose);

		Size = new Size(1296, 99);

		//  Set colors
		BackColor = background_color;
		topScore.ForeColor = text_color_total;
		sobScore.ForeColor = text_color_total;



		SetControls();


		//  Redraw the form if the window is resized
		Resize += delegate { DoLayout(); };
		Move += delegate { DoLayout();};

		//  Draw the form
		DoLayout();

		sInput.Show();
		Show();

		//  When the form is shown set the focus to the input box

		//  Close the network connection when the form is closed
		//  To prevent any hangups
		//FormClosing += delegate { CloseNetwork(); };
	}

	//  Just ripped the following 2 methods from dumas's code
	private int GetWidth()
	{
		return (
				Width - (2 * SystemInformation.FrameBorderSize.Width)
		       );
	}

	private int GetHeight()
	{
		return (
				Height - (2 * SystemInformation.FrameBorderSize.Height +
					SystemInformation.CaptionHeight)
		       );
	}

	private void SetControls()
	{
		Controls.Clear();

		FileReader run = pbEasy;
		if (config["hard_route"] == "1")
		{
			run = pbHard;
		}
		try
		{
			int total = 0;
			int sob = 0;
			foreach(KeyValuePair<string, string> level in run)
			{
				int sc = Int32.Parse(level.Value);
				total += sc;
				Score newScore = new Score(level.Key, sc);
				levels.Controls.Add(newScore);

			}

			int i = 0;
			foreach(KeyValuePair<string, string> level in individualLevels)
			{
				Score.SetBest(level.Key, Int32.Parse(level.Value), i);
				i++;
			}

			foreach(Score s in Score.scoresList)
			{
				sob += s.best;
			}
			topScore.Text = "Top: " + total;
			totals.Controls.Add(topScore);
			sobScore.Text = "SoB: " + sob;
			totals.Controls.Add(sobScore);
			
			if (config["sums_horizontal_alignment"] == "left")
			{
				Controls.Add(totals);
				Controls.Add(levels);
			}
			else
			{
				Controls.Add(levels);
				Controls.Add(totals);
			}

		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}
		DoLayout();
	}

	public void DoLayout()
	{
		totals.Width = 310;
		levels.Width = GetWidth() - totals.Width;
		DoTotalsLayout();
		DoLevelsLayout();
		
		if (config["sums_horizontal_alignment"] == "left")
		{
			levels.Left = totals.Width;
		}
		else
		{
			totals.Left = levels.Width;
		}
		
		//totals.Left = sList[sList.Count - 1].Left + 135;
		
		
		Refresh();
	}
	
	public void DoTotalsLayout()
	{
		topScore.Width = 155;
		topScore.Height = GetHeight();
		sobScore.Left = topScore.Left + 155;
		sobScore.Width = 155;
		sobScore.Height = GetHeight();
	}
	
	public void DoLevelsLayout()
	{
		List<Score> sList = Score.scoresList;
		foreach (Score s in sList)
		{
			s.Height = GetHeight();
			s.Width = levels.Width / 7;
		}

		for (int i = 1; i < sList.Count; i++)
		{
			sList[i].Left = sList[i-1].Left + levels.Width / 7;
		}
	}
	
	public void ConfirmClose(object sender, FormClosingEventArgs e) 
	{
			if (sInput.index > 0 && !closing)
			{
				var confirmResult =  MessageBox.Show("Your run is incomplete! Are you sure you wish to exit?\n\n(Any unsaved gold scores will be lost)\n(To save gold scores on an incomplete run fill in the rest of the levels with 0)",
                                     "Continue Closing?",
                                     MessageBoxButtons.YesNo);
				if (confirmResult == DialogResult.Yes)
				{
					closing = true;
					Application.Exit();
				}
				else
				{
					e.Cancel = true;
				}
			}
			else
			{
				closing = true;
				Application.Exit();
			}	
	}






	public static void Main(string[] args)
	{
		try
		{
			var handle = GetConsoleWindow();
			ShowWindow(handle, SW_HIDE);
		}
		catch (Exception)
		{

		}

		for (int i = 0; i < args.Length; i++)
		{
			/*if (args[i] == "-s")
			{
				//  If -s is found in the arguments check if the port is specified
				for (int j = 0; j < args.Length; j++)
				{
					if (args[j] == "-p" && j < args.Length - 1)
					{
						//  If -p is found set port to the next argument
						port = Convert.ToInt32(args[j+1]);
					}
				}
				//  Start server and return from Main() before client is started
				StartServer(port);
				return;
			}*/
		}


		//  Start the client if -s was not found


		config = new FileReader("config.txt");
		config.AddNewItem("hard_route", "0");
		//config.AddNewItem("layout", "horizontal");
		config.AddNewItem("sums_horizontal_alignment", "right");
		config.AddNewItem("font", "Segoe UI");
		config.AddNewItem("font_size", "18");
		config.AddNewItem("start_highlighted", "1");
		config.AddNewItem("background_color", "#0F0F0F");
		config.AddNewItem("background_color_highlighted", "#0F0F0F");
		config.AddNewItem("text_color", "#FFFFFF");
		config.AddNewItem("text_color_highlighted", "#FFFFFF");
		config.AddNewItem("text_color_ahead", "#00CC36");
		config.AddNewItem("text_color_behind", "#CC1200");
		config.AddNewItem("text_color_best", "#D8AF1F");
		config.AddNewItem("text_color_total", "#FFFFFF");

		pbEasy = new FileReader("pb_easy.txt");
		pbEasy.AddNewItem("Cornaria", "0");
		pbEasy.AddNewItem("Meteo", "0");
		pbEasy.AddNewItem("Katina", "0");
		pbEasy.AddNewItem("Sector X", "0");
		pbEasy.AddNewItem("Macbeth", "0");
		pbEasy.AddNewItem("Area 6", "0");
		pbEasy.AddNewItem("Venom", "0");

		pbHard = new FileReader("pb_hard.txt");
		pbHard.AddNewItem("Cornaria", "0");
		pbHard.AddNewItem("Sector Y", "0");
		pbHard.AddNewItem("Aquas", "0");
		pbHard.AddNewItem("Zoness", "0");
		pbHard.AddNewItem("Macbeth", "0");
		pbHard.AddNewItem("Area 6", "0");
		pbHard.AddNewItem("Venom", "0");

		individualLevels = new FileReader("pb_individuals.txt");
		individualLevels.AddNewItem("Cornaria", "0");
		individualLevels.AddNewItem("Meteo", "0");
		individualLevels.AddNewItem("Katina", "0");
		individualLevels.AddNewItem("Sector X", "0");
		individualLevels.AddNewItem("Sector Y", "0");
		individualLevels.AddNewItem("Aquas", "0");
		individualLevels.AddNewItem("Zoness", "0");
		individualLevels.AddNewItem("Macbeth", "0");
		individualLevels.AddNewItem("Area 6", "0");
		individualLevels.AddNewItem("Venom", "0");

		try
		{
			config.Save();
			pbEasy.Save();
			pbHard.Save();
			individualLevels.Save();
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);

			//peanut_butter = "CO:0\nME:0\nKA:0\nSX:0\nMA:0\na6:0\nVE:0";
			//individual_levels = "CO:0\nME:0\nKA:0\nSX:0\nSY:0\nAQ:0\nZO:0\nMA:0\na6:0\nVE:0";
		}

		try
		{
			Application.Run(new ScoreTracker());
		}
		catch (Exception) {}
		/*
		try
		{
			var sw = new StreamWriter("foundElements.txt");

			int total = match(elementArray, blankList, sw);

			sw.WriteLine("\nTotal combinations: {0}", total);
			Console.WriteLine("\nTotal combinations: {0}", total);

			sw.Close();
		}
		catch (Exception e)
		{
			Console.WriteLine("Error: " + e.Message);
		}
		*/
	}
}
