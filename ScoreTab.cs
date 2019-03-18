using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

public class ScoreTab : Panel
{
	private FileReader file;

	private TabControl tabs = new TabControl();
	private Dictionary<string, ScorePage> pages = new Dictionary<string, ScorePage>();
	private TabPage comparisons = new TabPage();
	private TabControl comparisonsTabs = new TabControl();
	private ComparisonSelector selector;
	private Panel currentComparison = new Panel();
	private Button addComparison = new Button();
	private Button removeComparison = new Button();

	private static int tabIndex = 0;
	private static int selectorIndex = 0;

	public ScoreTab(FileReader file)
	{
		this.file = file;
		TrackerData.ValidateFile(file);

		Text = "Scores";
		Dock = DockStyle.Fill;

		Controls.Add(tabs);
		
		addComparison.Dock = DockStyle.Left;
		addComparison.Text = "Add Comparison";
		removeComparison.Dock = DockStyle.Right;
		removeComparison.Text = "Remove Comparison";

		Panel p = new Panel();
		p.Height = 20;
		p.Controls.Add (addComparison);
		p.Controls.Add (removeComparison);
		p.Dock = DockStyle.Top;

		pages.Add("Best Run", ConfigureTab("Best Run"));
		pages.Add("Top Scores", ConfigureTab("Top Scores"));
		tabs.TabPages.Add(ToTabPage(pages["Best Run"]));
		tabs.TabPages.Add(ToTabPage(pages["Top Scores"]));
		comparisons.Text = "Comparisons";
		selector = new ComparisonSelector(file);
		selector.JustComparisons = true;
		selector.Reload();
		selector.Dock = DockStyle.Top;
		currentComparison.Dock = DockStyle.Fill;
		comparisons.Controls.Add(currentComparison);
		comparisons.Controls.Add(selector);
		comparisons.Controls.Add(p);

		selector.Changed = ReloadComparisons;
		selector.Reloaded = ReloadComparisons;
		selector.Index = selectorIndex;

		ReloadComparisons();

		tabs.TabPages.Add(comparisons);

		tabs.SelectedIndexChanged += delegate { CacheTabIndex(); };
		addComparison.Click += delegate { AddComparison(); };
		removeComparison.Click += delegate { ConfirmComparisonDeletion(); };

		Layout += new LayoutEventHandler((object sender, LayoutEventArgs e) => DoLayout());

		tabs.SelectedIndex = tabIndex;

		DoLayout();
	}

	public void CacheTabIndex()
	{
		tabIndex = tabs.SelectedIndex;

	}

	public void CacheComparisonIndex()
	{
		selectorIndex = selector.Index;
	}

	public void Save()
	{
		foreach (KeyValuePair<string, ScorePage> pair in pages)
		{
			pair.Value.SaveScores();
		}
		file.Save();
	}

	public void DoLayout()
	{
		tabs.Dock = DockStyle.Fill;
		foreach (KeyValuePair<string, ScorePage> pair in pages)
		{
			pair.Value.DoLayout();
		}
		addComparison.Width = ClientRectangle.Width/2;
		removeComparison.Width = addComparison.Width;
	}

	private TabPage ToTabPage(ScorePage page)
	{
		TabPage toReturn = new TabPage();
		toReturn.Text = page.Text;
		toReturn.Controls.Add(page);
		return toReturn;
	}

	private void ReloadComparisons()
	{
		foreach (string section in file.Sections)
		{
			if (section == "Best Run" || section == "Top Scores" || section == "General" || section == "Sum of Best")
				continue;

			if (!pages.ContainsKey(section))
				pages.Add(section, ConfigureTab(section));
		}
		currentComparison.Controls.Clear();
		if (selector.Count > 0)
		{
			currentComparison.Controls.Add(pages[selector.Comparison]);
			removeComparison.Enabled = true;
		}
		else
		{
			removeComparison.Enabled = false;
		}
		CacheComparisonIndex();
	}

	private ScorePage ConfigureTab (string section)
	{
		ScorePage page = new ScorePage(file, section);
		foreach (KeyValuePair<string, string> score in file.GetSection(section))
		{

			page.Add(new NumericField (score.Key, score.Value));

		}
		return page;
	}
	
	public void RemoveComparison(string name)
	{
		if (name == "Best Run" || name == "Top Scores" || name == "Sum of Best" || name == "General")
			return;

		file.RemoveSection(name);
		pages.Remove(name);
		selector.Reload();
		//ReloadComparisons();
	}

	public void AddComparison()
	{
		AskName popup = new AskName();
		popup.ShowDialog();
		string name = popup.Name;
		if (name == "Best Run" || name == "Top Scores" || name == "Sum of Best" || name == "General")
			return;

		foreach (string key in file.GetSection("Best Run").Keys)
		{
			file.AddNewItem(name, key, "0");
		}
		selector.Reload();
		selector.Index = selector.GetIndexOfComparison(name);
	}

	public void ConfirmComparisonDeletion()
	{

		var confirmResult = MessageBox.Show ("Are you sure you wish to delete your \"" + selector.Comparison +"\" comparison?",
				"Continue removing \"" + selector.Comparison +"\"?",
				MessageBoxButtons.YesNo);
		if (confirmResult == DialogResult.Yes)
		{
			if (selector.Count > 0) 
			RemoveComparison(selector.Comparison);
		}
	}
}

public class AskName : Form
{
	private class NameTextBox : TextBox
	{
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			// Check if the pressed character was a backspace or numeric.
			e.Handled = !(
					e.KeyChar == (char)8 ||
					Char.IsWhiteSpace(e.KeyChar) ||
					Char.IsLetterOrDigit(e.KeyChar)
				     );
		}
	}

	public string Name 
	{ 
		get
		{
			return text.Text;
		}
	}
	private Button submit = new Button();
	private NameTextBox text = new NameTextBox();

	public AskName()
	{
		Text = "New Comparison Name?";
		text.Dock = DockStyle.Fill;
		submit.Text = "Submit";
		submit.Dock = DockStyle.Bottom;

		Controls.Add(text);
		Controls.Add(submit);

		Height = GetCaptionSize() + text.Height + submit.Height;

		submit.Click += delegate { Close(); };
	}
	private int GetCaptionSize()
	{
		return (2 * SystemInformation.FrameBorderSize.Height + SystemInformation.CaptionHeight);
	}
}
