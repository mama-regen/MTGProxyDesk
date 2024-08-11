﻿using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace MTGProxyDesk
{
    public partial class StartPage : Page
    {
        MagicDeck magicDeck;
        string filePath = "";

        public StartPage()
        {
            InitializeComponent();
            magicDeck = MagicDeck.Instance;
        }

        public void BrowseDeck(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = Path.Join(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "decks");
            ofd.Filter = "MTG Proxy Deck (*.mpd)|*.mpd|All Files (*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == true)
            {
                filePath = ofd.FileName;
                DeckName.Text = Path.GetFileName(filePath);
                NewDeckButton.Visibility = Visibility.Hidden;
                EditDeckButton.Visibility = Visibility.Visible;
                EditDeckButton.IsEnabled = true;
                PlayButton.Visibility = Visibility.Visible;
                //PlayButton.IsEnabled = true;
            }
        }

        public void EditDeck(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Content.ToString() == "Edit Deck")
            {
                magicDeck.Load(filePath);
            }
            NewDeckPage newDeck = new NewDeckPage();
            this.NavigationService.Navigate(newDeck);
        }

        public void BeginPlay(object sender, RoutedEventArgs e)
        {

        }
    }
}
