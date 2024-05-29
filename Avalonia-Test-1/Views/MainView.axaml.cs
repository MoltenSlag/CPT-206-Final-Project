using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia_Test_1.Models;
using System;
using System.Timers;

namespace Avalonia_Test_1.Views;

public partial class MainView : UserControl
{
    //Arrays for picture sources and tracking the reel
    Avalonia.Media.IImage[] picSourceArray = new Avalonia.Media.IImage[7];
    int[,] reelArray = new int[3, 3];

    int points = 100;
    string currentUser = "Guest";
    public bool loggedIn = false;

    //Load pictures as images
    Bitmap circle = new Bitmap(AssetLoader.Open(new Uri("avares://Avalonia-Test-1/Assets/circle.png")));
	Bitmap triangle = new Bitmap(AssetLoader.Open(new Uri("avares://Avalonia-Test-1/Assets/triangle.png")));
	Bitmap square = new Bitmap(AssetLoader.Open(new Uri("avares://Avalonia-Test-1/Assets/square.png")));
	Bitmap pentagon = new Bitmap(AssetLoader.Open(new Uri("avares://Avalonia-Test-1/Assets/pentagon.png")));
	Bitmap hexagon = new Bitmap(AssetLoader.Open(new Uri("avares://Avalonia-Test-1/Assets/hexagon.png")));
	Bitmap octagon = new Bitmap(AssetLoader.Open(new Uri("avares://Avalonia-Test-1/Assets/octagon.png")));
	Bitmap star = new Bitmap(AssetLoader.Open(new Uri("avares://Avalonia-Test-1/Assets/star.png")));

    //Making timers for animations
    //Techniaclly not used because of bugs, but leaving them uncommented
    //because their impact is insignificant
    Timer reel1SpinInterval = new Timer(100);
	Timer reel2SpinInterval = new Timer(100);
	Timer reel3SpinInterval = new Timer(100);
	Timer reel1Stop = new Timer(3000);
    Timer reel2Stop = new Timer(3500);
    Timer reel3Stop = new Timer(4000);

    //Making a user authentication class to interact with tables
    UserAuthentication userAuth = new UserAuthentication();

	public MainView()
    {
        InitializeComponent();
        picSourceArray = [circle, triangle, square, pentagon, hexagon, octagon, star];
        PointsText.Text = $"Points: {points}";
        UsernameText.Text = $"{currentUser}";
        //Set the functions that happen when the timers elapse
        //Technically not used because of bugs, but leaving them uncommented
        //because their impact is insignificant
        reel1SpinInterval.Elapsed += Reel1Roll;
        reel2SpinInterval.Elapsed += Reel2Roll;
        reel3SpinInterval.Elapsed += Reel3Roll;
        reel1Stop.Elapsed += Reel1StopFunc;
        reel2Stop.Elapsed += Reel2StopFunc;
        reel3Stop.Elapsed += Reel3StopFunc;
    }

    //The next two functions simply switch button states
    public void OnButton5Click(object source, RoutedEventArgs args)
    {
        button5.IsEnabled = false;
        button10.IsEnabled = true;
        buttonSpin.IsEnabled = true;
    }

    public void OnButton10Click(object source, RoutedEventArgs args)
    {
        button5.IsEnabled = true;
        button10.IsEnabled = false;
        buttonSpin.IsEnabled = true;
    }

    public void OnButtonSpinClick(object source, RoutedEventArgs args)
    {
		int[] topLine = new int[3];
		int[] centerLine = new int[3];
		int[] bottomLine = new int[3];

        //Uses the button state to determine the course of action
        if (!button5.IsEnabled)
        {
            points -= 5;
			PointsText.Text = $"Points: {points}";
			topLine = GenerateNonPaylineResults();
            centerLine = GeneratePaylineResults();
            bottomLine = GenerateNonPaylineResults();
        }
        if (!button10.IsEnabled)
        {
            points -= 10;
			PointsText.Text = $"Points: {points}";
			topLine = GeneratePaylineResults();
            centerLine = GeneratePaylineResults();
            bottomLine = GeneratePaylineResults();
        }

        button5.IsEnabled = true;
        button10.IsEnabled = true;
        buttonSpin.IsEnabled = false;

        reelArray[0, 0] = topLine[0];
        reelArray[0, 1] = topLine[1];
        reelArray[0, 2] = topLine[2];
        reelArray[1, 0] = centerLine[0];
        reelArray[1, 1] = centerLine[1];
        reelArray[1, 2] = centerLine[2];
        reelArray[2, 0] = bottomLine[0];
        reelArray[2, 1] = bottomLine[1];
        reelArray[2, 2] = bottomLine[2];

        //The below commented-out function is currently unusable due to throwing an exception
        //ReelAnimationFunc();

        //The below saves the results to the database
        if (loggedIn) { userAuth.UpdateUserPoints(currentUser, points); }

        //The following set the images based on the generated paylines
        reel11.Source = picSourceArray[reelArray[0, 0]];
        reel12.Source = picSourceArray[reelArray[1, 0]];
        reel13.Source = picSourceArray[reelArray[2, 0]];
        reel21.Source = picSourceArray[reelArray[0, 1]];
        reel22.Source = picSourceArray[reelArray[1, 1]];
        reel23.Source = picSourceArray[reelArray[2, 1]];
        reel31.Source = picSourceArray[reelArray[0, 2]];
        reel32.Source = picSourceArray[reelArray[1, 2]];
        reel33.Source = picSourceArray[reelArray[2, 2]];
        PointsText.Text = $"Points: {points}";
	}

    public void OnButtonLoginClick(object source, RoutedEventArgs args)
    {
        //If the user is not logged in, calls the database and tries to pull the user
        //If the user is logged in, transforms into a 'logout' button that saves the user's points and
        //reinstates the 'guest' account at default points
        int userPoints = -1;
        bool failCheck = false;
        if(UsernameEntry.Text == null && !loggedIn || UsernameEntry.Text.Length == 0 && !loggedIn)
        {
            ErrorWindow errorWindow = new ErrorWindow("Please enter a username.");
            errorWindow.Show();
            return;
        }
        if(PasswordEntry.Text == null && !loggedIn || PasswordEntry.Text.Length == 0 && !loggedIn)
        {
            ErrorWindow errorWindow = new ErrorWindow("Please enter a password.");
            errorWindow.Show();
            return;
        }
        if(!loggedIn)
        {
            userPoints = userAuth.AuthenticateUser(UsernameEntry.Text, PasswordEntry.Text);
            if (userPoints == -1) { failCheck = true; }
            if (failCheck) { return; }
            currentUser = UsernameEntry.Text;
            points = userPoints;
            PointsText.Text = $"Points: {points}";
            ButtonLogin.Content = "Logout";
        }
        if(loggedIn)
        {
            failCheck = !userAuth.UpdateUserPoints(currentUser, points);
            if (failCheck) { return; }
            currentUser = "Guest";
            points = 100;
			PointsText.Text = $"Points: {points}";
			ButtonLogin.Content = "Login";
        }
        loggedIn = !loggedIn;
        UsernameText.Text = currentUser;
        UsernameEntry.Text = "";
        PasswordEntry.Text = "";
        FlyoutLoginGrid.IsVisible = !FlyoutLoginGrid.IsVisible;
        ButtonRegister.IsVisible = !ButtonRegister.IsVisible;
    }

    public void OnButtonRegisterClick(object source, RoutedEventArgs args)
    {
        //This button tries to create a new user in the database
        //When logged in, this button is not available, but there is an escape in case it is somehow pressed
        bool failCheck = false;
        if (loggedIn) { return; }
		if (UsernameEntry.Text == null && !loggedIn || UsernameEntry.Text.Length == 0 && !loggedIn)
		{
			ErrorWindow errorWindow = new ErrorWindow("Please enter a username.");
			errorWindow.Show();
			return;
		}
		if (PasswordEntry.Text == null && !loggedIn || PasswordEntry.Text.Length == 0 && !loggedIn)
		{
			ErrorWindow errorWindow = new ErrorWindow("Please enter a password.");
			errorWindow.Show();
			return;
		}
        if (PasswordEntry.Text.Length < 6 && !loggedIn)
        {
			ErrorWindow errorWindow = new ErrorWindow("Password must be at least 6 characters.");
			errorWindow.Show();
			return;
		}
        failCheck = !userAuth.AddUser(UsernameEntry.Text, PasswordEntry.Text, points);
        if (failCheck) { return; }
        loggedIn = !loggedIn;
        currentUser = UsernameEntry.Text;
        UsernameText.Text = currentUser;
        UsernameEntry.Text = "";
        PasswordEntry.Text = "";
        ButtonLogin.Content = "Logout";
		FlyoutLoginGrid.IsVisible = !FlyoutLoginGrid.IsVisible;
		ButtonRegister.IsVisible = !ButtonRegister.IsVisible;
	}

    public int[] GeneratePaylineResults()
    {
        int[] results = new int[3];
        Random random = new Random();

        //The following code sets the odds for each payout result,
        //and sets the array so that the correct icons show
        double odds = random.NextDouble();

        if (odds < 0.02) 
        { 
            results = [6, 6, 6];
            points += 100;
        }
        else if (odds < 0.04) 
        {
            results = [5, 5, 5];
            points += 80;
        }
        else if (odds < 0.06)
        {
            results = [4, 4, 4];
            points += 60;
        }
        else if (odds < 0.08)
        {
            results = [3, 3, 3];
            points += 50;
        }
        else if (odds < 0.1)
        {
            results = [2, 2, 2];
            points += 40;
        }
        else if (odds < 0.12)
        {
            results = [1, 1, 1];
            points += 30;
        }
        else if (odds < 0.14)
        {
            results = [0, 0, 0];
            points += 20;
        }
        else if (odds < 0.3)
        {
            //This code ensures that there's at least one star in the line
            //but that there's not three stars, which would be a different payout
            results = [random.Next(7), random.Next(7), random.Next(7)];
            if (results[0] != 6 && results[1] != 6 && results[2] != 6)
            {
                results[random.Next(3)] = 6;
            }
			while (results[0] == results[1] && results[1] == results[2])
			{
				results[random.Next(3)] = random.Next(6);
			}
            points += 10;
		}
        else
        {
            //This code ensures that there's no line that should pay out when the
            //player doesn't win
            results = [random.Next(6), random.Next(6), random.Next(6)];
            while (results[0] == results[1] && results[1] == results[2])
            {
                results[random.Next(3)] = random.Next(6);
            }
        }

        return results;
    }

    public int[] GenerateNonPaylineResults()
    {
        //This code is used to generate random lines when the line doesn't pay out
        int[] results = new int[3];
        Random random = new Random();

        results = [random.Next(7), random.Next(7), random.Next(7)];
        return results;
    }

    //The below code is not currently in use because of threading-related bugs
    //TODO: Figure out how to fix this

    public void Reel1Roll(object source, ElapsedEventArgs e)
    {
        Random random = new Random();
		reel11.Source = picSourceArray[random.Next(7)];
		reel12.Source = picSourceArray[random.Next(7)];
		reel13.Source = picSourceArray[random.Next(7)];
	}

	public void Reel2Roll(object source, ElapsedEventArgs e)
	{
		Random random = new Random();
		reel21.Source = picSourceArray[random.Next(7)];
		reel22.Source = picSourceArray[random.Next(7)];
		reel23.Source = picSourceArray[random.Next(7)];
	}

	public void Reel3Roll(object source, ElapsedEventArgs e)
	{
		Random random = new Random();
		reel31.Source = picSourceArray[random.Next(7)];
		reel32.Source = picSourceArray[random.Next(7)];
		reel33.Source = picSourceArray[random.Next(7)];
	}

	public void Reel1StopFunc(object source, ElapsedEventArgs e)
    {
        reel1SpinInterval.Stop();
        reel1Stop.Stop();
		reel11.Source = picSourceArray[reelArray[0, 0]];
		reel12.Source = picSourceArray[reelArray[1, 0]];
		reel13.Source = picSourceArray[reelArray[2, 0]];
	}

	public void Reel2StopFunc(object source, ElapsedEventArgs e)
	{
        reel2SpinInterval.Stop();
		reel2Stop.Stop();
		reel21.Source = picSourceArray[reelArray[0, 1]];
		reel22.Source = picSourceArray[reelArray[1, 1]];
		reel23.Source = picSourceArray[reelArray[2, 1]];
	}

	public void Reel3StopFunc(object source, ElapsedEventArgs e)
	{
        reel3SpinInterval.Stop();
		reel3Stop.Stop();
		reel31.Source = picSourceArray[reelArray[0, 2]];
		reel32.Source = picSourceArray[reelArray[1, 2]];
		reel33.Source = picSourceArray[reelArray[2, 2]];
	}

    public void ReelAnimationFunc()
    {
        reel1SpinInterval.Start();
        reel2SpinInterval.Start();
        reel3SpinInterval.Start();
        reel1Stop.Start();
        reel2Stop.Start();
        reel3Stop.Start();
    }
}
