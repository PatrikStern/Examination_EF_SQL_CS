using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EFLibrary.Models;
using EFLibrary;
using Microsoft.Win32;
using System.Collections;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool? location = null;
        bool allDays = false;
        bool tempOrHumidity = false;
        public MainWindow()
        {
            InitializeComponent();
        }


        private void ReadfileButton_Click(object sender, RoutedEventArgs e)
        {
            int post = SQLService.FileReader(FilepathTxt.Text);
            Output.Text = post.ToString() + " inserted row/s to database.";
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            string file;
            OpenFileDialog openFile = new OpenFileDialog();
            if(openFile.ShowDialog() == true)
            {
                file = openFile.FileName;
                FilepathTxt.Text = file;
            }
            else
            {
                FilepathTxt.Text = "Choose functional file";
            }
        }

        private void MidTempSearcher_Click(object sender, RoutedEventArgs e)
        {
            
            if (location == null)
            {
                Output.Text = "Du måste välja om din sökning skall göras inomhus eller utomhus.";
            }
            else if (location == false && allDays == false)
            {

                double? post;
                DateTime? calenderoutput = Calender.SelectedDate;
                if (calenderoutput == null)
                {
                    Output.Text = "Välj ett datum att söka på i kalendern.";
                }
                else
                {
                    var calenderParam = calenderoutput.Value.Date.ToShortDateString();
                    post = SQLService.midtempDate(calenderParam, location);
                    Output.Text = $"{calenderParam} var medeltemperaturen ute {post} grader.";
                }
            }

            else if (location == true && allDays == false)
            {
                double? post;
                DateTime? calenderoutput = Calender.SelectedDate;
                if (calenderoutput == null)
                {
                    Output.Text = "Välj ett datum att söka på i kalendern.";
                }
                else
                {
                    var calenderParam = calenderoutput.Value.Date.ToShortDateString();
                    post = SQLService.midtempDate(calenderParam, location);
                    Output.Text = $"{calenderParam} var medeltemperaturen inne {post} grader.";
                }
            }

            else if(allDays == true && tempOrHumidity == false)
            {
                int topthreecounter = 0;
                if (location == null)
                {
                    Output.Text = "Du måste välja om din sökning skall göras inomhus eller utomhus.";
                }
                Output.Text = "";

                foreach(var tempature in SQLService.TopColdestHottest(location, tempOrHumidity))
                {
                    if(topthreecounter == 0)
                    {
                        Output.Text += "Varmaste dagarna:\n";
                    }
                    else if (topthreecounter == 3)
                    {
                        Output.Text += "\nKallaste dagarna:\n";
                    }

                    Output.Text += $"{tempature.Date.Value.Date.ToShortDateString()} {tempature.Level} grader.\n";
                    topthreecounter++;
                }
               
            }
           
        }

        private void Outsideradio_Checked(object sender, RoutedEventArgs e)
        {
            location = false;
        }

        private void Insideradio_Checked(object sender, RoutedEventArgs e)
        {
            location = true;
        }

        private void HumiditySearchButton_Click(object sender, RoutedEventArgs e)
        {
            Output.Text = "";
            int topthreecounter = 0;
            if (location == null)
            {
                Output.Text = "Du måste välja om din sökning skall göras inomhus eller utomhus.";
            }
            else
            {
                tempOrHumidity = true;
                foreach (var humidity in SQLService.TopColdestHottest(location, tempOrHumidity))
                {
                    if (topthreecounter == 0)
                    {
                        Output.Text += "Fuktigaste dagar:\n";
                    }
                    else if (topthreecounter == 3)
                    {
                        Output.Text += "\nMinst fuktiga dagar:\n";
                    }
                    Output.Text += $"{humidity.Date.Value.Date.ToShortDateString()} {humidity.Level}% luftfuktighet.\n";
                    topthreecounter++;
                }
                tempOrHumidity = false;
            }
            
        }

        private void alldaychecker_Checked(object sender, RoutedEventArgs e)
        {
            allDays = true;
        }

        private void alldaychecker_Unchecked(object sender, RoutedEventArgs e)
        {
            allDays = false;
        }

        private void Moleriskbutton_Click(object sender, RoutedEventArgs e)
        {
            Output.Text = "";
            int topthreecounter = 0;
            if (location == null)
            {
                Output.Text = "Du måste välja om din sökning skall göras inomhus eller utomhus.";
            }
            else
            {
                foreach(var molerisk in SQLService.MoleRisk(location))
                {
                    if(molerisk.Level < 0)  //"To correct" the returing list with negative numbers, list will be sorted from biggest to smallest number in returing list but corrected to a 0% if number is negative.
                    {
                        molerisk.Level = 0;
                    }
                    if (topthreecounter == 0)
                    {
                        Output.Text += "Högst mögelrisk:\n";
                    }
                    else if (topthreecounter == 3)
                    {
                        Output.Text += "\nMinst mögelrisk:\n";
                    }
                    Output.Text += $"{molerisk.Date.Value.Date.ToShortDateString()} var mögelrisken {molerisk.Level}%\n";
                    topthreecounter++;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Output.Text = "";
            foreach (var season in SQLService.MeteorologicalSeason())
            {
                if(season.Info == "Höst")
                {
                    Output.Text = $"Meterologisk höst var {season.Date.Value.Date.ToShortDateString()} med en utomhus temperatur på {season.Level} grader.\n\n";
                }
                else if(season.Info == "Vinter")
                {
                    Output.Text = $"Meterologisk vinter var {season.Date.Value.Date.ToShortDateString()} med en utomhus temperatur på {season.Level} grader.\n\n";
                }  
            }
            //From the basis that 5 days needs to be equal or below 0 degrees to count for metrological winter the information of the data don't apply for that, so the date don't exist. 
        }
    }
}
