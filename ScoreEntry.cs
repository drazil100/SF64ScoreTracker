using System;
using System.Collections.Generic;

public class ScoreEntry
{
	private string name = "";
	private int score = -1;
	private int comparison = 0;
	private int pace = 0;
	private int position = 0;
	private PaceStatus status = PaceStatus.Default;
	private bool isCurrent = false;

	public string Name
	{
		get { return name; }
		private set { name = value; }
	}

	public int Score
	{
		get { return (IsSet) ? score : comparison; }
		set 
		{ 
			score = value;
			if (value < 0) score = -1; 
			if (score == -1)
			{
				Status = PaceStatus.Default;
				Pace = 0;
			}
		}
	}

	public int Comparison
	{
		get { return comparison; }
		set { comparison = value; }
	}

	public int Pace
	{
		get { return pace; }
		set { pace = value; }
	}

	public PaceStatus Status
	{
		get { return status; }
		set { status = value; }
	}

	public bool IsSet
	{
		get { return (score > -1) ? true : false; }
	}
	public bool IsCurrent
	{
		get { return isCurrent; }
		set { isCurrent = value; }
	}

	public int Position
	{
		get { return position; }
		set { position = value; }
	}
	
	public ScoreEntry(string name, int comparisonScore)
	{
		this.name = name;
		this.comparison = comparisonScore;
	}
	
	public override string ToString()
	{
		return String.Format("{0}: {1}", Name, Score);
	}
}
