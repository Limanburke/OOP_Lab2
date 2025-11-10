using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OOP_Lab2.Parsers;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace OOP_Lab2.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private string xmlPath = "";
        private string xslPath = "";

        private Dictionary<string, ObservableCollection<string>> attributeValues = new();
        public Dictionary<string, ObservableCollection<string>> AttributeValues
        {
            get => attributeValues;
            set => SetProperty(ref attributeValues, value);
        }

        public List<string> AttributeNames => AttributeValues.Keys.ToList();

        [ObservableProperty] private ObservableCollection<string> attributeValues1 = new();
        [ObservableProperty] private ObservableCollection<string> attributeValues2 = new();
        [ObservableProperty] private ObservableCollection<string> attributeValues3 = new();
        [ObservableProperty] private ObservableCollection<string> attributeValues4 = new();
        [ObservableProperty] private ObservableCollection<string> attributeValues5 = new();

        [ObservableProperty] private string selectedValue1;
        [ObservableProperty] private string selectedValue2;
        [ObservableProperty] private string selectedValue3;
        [ObservableProperty] private string selectedValue4;
        [ObservableProperty] private string selectedValue5;

        [ObservableProperty] private int pickerIndex1 = -1;
        [ObservableProperty] private int pickerIndex2 = -1;
        [ObservableProperty] private int pickerIndex3 = -1;
        [ObservableProperty] private int pickerIndex4 = -1;
        [ObservableProperty] private int pickerIndex5 = -1;

        [ObservableProperty] private string textToView;
        [ObservableProperty] private string pickerAttributeText = "Upload file to view attributes";

        [ObservableProperty] private string selectedType = "SAX API";

        [ObservableProperty]
        private ObservableCollection<string> types = new ObservableCollection<string>
        {
               "SAX API",
               "DOM API",
               "LINQ to XML"
        };

        private Parser parser;

        [RelayCommand]
        private async Task ShowClicked()
        {
            await Shell.Current.DisplayAlert("Information about project", "Project was done by\nStudent\nGroup K-24\nEugene Kryvushko", "Ok");
        }

        [RelayCommand]
        async Task LoadXmlFile()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Pick XML File",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> { { DevicePlatform.WinUI, new[] { ".xml" } } })
                });
                if (file != null)
                {
                    xmlPath = file.FullPath;
                    await Shell.Current.DisplayAlert("Success", "Your XML file was added successfully", "OK");
                    PickerAttributeText = "Select Attribute";
                    switch (selectedType)
                    {
                        case "SAX API":
                            {
                                parser = new Parser(new SaxParser(this));
                                break;
                            }
                        case "DOM API":
                            {
                                parser = new Parser(new DomParser(this));
                                break;
                            }
                        case "LINQ to XML":
                            {
                                parser = new Parser(new LinqToXmlParser(this));
                                break;
                            }
                    }

                    if (parser != null)
                    {
                        var result = parser.doParse(xmlPath);
                        AttributeValues = result;
                        OnPropertyChanged(nameof(AttributeValues));
                        OnPropertyChanged(nameof(AttributeNames));

                        if (result.Count >= 1) AttributeValues1 = result.ElementAt(0).Value;
                        if (result.Count >= 2) AttributeValues2 = result.ElementAt(1).Value;
                        if (result.Count >= 3) AttributeValues3 = result.ElementAt(2).Value;
                        if (result.Count >= 4) AttributeValues4 = result.ElementAt(3).Value;
                        if (result.Count >= 5) AttributeValues5 = result.ElementAt(4).Value;
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Problems in loading XML file", $"The XML file couldn't be loaded: {ex}", "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task LoadXslFile()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> { { DevicePlatform.WinUI, new[] { ".xsl" } } })
                });

                if (file != null)
                {
                    xslPath = file.FullPath;
                    await Shell.Current.DisplayAlert("Success", "Your XSL file was added successfully", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Problems in loading XSL file", $"The XSL file couldn't be loaded: {ex}", "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task Search()
        {
            if(selectedValue1 ==  null && selectedValue2 == null && selectedValue3 == null && selectedValue4 == null && selectedValue5 == null)
            {
                await Shell.Current.DisplayAlert("Problems in Searching", $"Select attribute before searching", "Ok");
                return;
            }
            parser.doSearchResult(selectedValue1, selectedValue2, selectedValue3, selectedValue4, selectedValue5);
            return;
        }

        [RelayCommand]
        void ToHTML()
        {
            Transform();
        }

        async void Transform()
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();

                xslt.Load(xslPath);
                string f3 = getFilePath("schedule.html");

                xslt.Transform(xmlPath, f3);
                await Shell.Current.DisplayAlert("Transformation to HTML", "XML successfully transformed\nHTML file created on your desktop", "Ok");
            }
            catch (ArgumentException)
            {
                await Shell.Current.DisplayAlert("Transformation to HTML", "The xml file was not loaded. Please load xml file before transforming.", "Ok");
            }
            catch (InvalidOperationException)
            {
                await Shell.Current.DisplayAlert("Transformation to HTML", "Unable to find xsl file. Please load xsl file before transforming.", "Ok");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Transformation to HTML", $"Error during transformation:\n {ex}", "Ok");
            }
        }

        private static string getFilePath(string fileName)
        {
            return Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.Desktop), fileName);
        }


        [RelayCommand]
        async void Exit()
        {
            bool choice = await Shell.Current.DisplayAlert("Exit message", "Exit program?", "Yes", "No");
            if (choice)
            {
                Application.Current?.Quit();
            }
        }

        [RelayCommand]
        void Clear()
        {
            PickerIndex1 = -1;
            PickerIndex2 = -1;
            PickerIndex3 = -1;
            PickerIndex4 = -1;
            PickerIndex5 = -1;
            TextToView = null;
        }
    }
}

