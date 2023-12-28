using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using warframeParse.Models.Entities;
using warframeParse.Models.ViewModels;

namespace warframeParse
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public ViewModel()
        {
            wfList = GetWFFromDatabase();
            SelectedItem = new WarframeVM();
            ISWFDetails = false;
            ISDeleteWF = false;
            ProgressVis = "Hidden";
            IsParsing = true;
        }
        private bool iswfDetails;
        public bool ISWFDetails
        {
            get
            {
                return iswfDetails;
            }
            set
            {
                iswfDetails = value;
                OnPropertyChanged();
            }
        }
        private bool isdeleteWf;
        public bool ISDeleteWF
        {
            get
            {
                return isdeleteWf;
            }
            set
            {
                isdeleteWf = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<WarframeVM> wfList;
        public ObservableCollection<WarframeVM> WfList
        {
            get
            {
                return wfList;
            }
            set
            {
                wfList = value;
                OnPropertyChanged();
            }
        }
        private Command getWF;
        public Command GetWF
        {
            get
            {
                getWF = new Command(obj =>
                {
                    WfList = GetWFFromDatabase();
                });
                return getWF;
            }
        }

        private ObservableCollection<WarframeVM> GetWFFromDatabase() {
            var test = new ObservableCollection<WarframeVM>();
            using (var db = new warEntities())
            {
                var wfInDB = db.warframe.OrderBy(x => x.name).Include(g => g.warframe_polarity.Select(f => f.Polarity)).ToList();
                var polList = db.Polarity.OrderBy(x => x.name).ToList();
                // m n v

                test = new ObservableCollection<WarframeVM>(wfInDB.Select(x => new WarframeVM
                {
                    Name = x.name,
                    Sex = x.sex,
                    Health = x.health,
                    Shields = x.shields,
                    Armor = x.armor,
                    Energy = x.energy,
                    Sprint_speed = x.sprint_speed,
                    Element = x.element.name,
                    Madurai = x.warframe_polarity.Where(y => y.Polarity.id == polList[0].id).FirstOrDefault() ?? new warframe_polarity { warframe_id = x.id, polarity_id = polList[0].id },
                    Naramon = x.warframe_polarity.Where(y => y.Polarity.id == polList[1].id).FirstOrDefault() ?? new warframe_polarity { warframe_id = x.id, polarity_id = polList[1].id },
                    Vazarin = x.warframe_polarity.Where(y => y.Polarity.id == polList[2].id).FirstOrDefault() ?? new warframe_polarity { warframe_id = x.id, polarity_id = polList[2].id },
                }));

            }
            return test;
        }
        private int selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
                OnPropertyChanged();
            }
        }

        private WarframeVM selctedItem;
        public WarframeVM SelectedItem
        {
            get
            {
                return selctedItem;
            }
            set
            {
                selctedItem = value;
                OnPropertyChanged();
                if (selctedItem != null)
                {
                    ISDeleteWF = true;
                    ISWFDetails = true;
                }
                else
                {
                    ISDeleteWF = false;
                    ISWFDetails = false;
                }
            }
        }
        private warframe frameDB;
        public warframe FrameDB
        {
            get
            {
                return frameDB;
            }
            set
            {
                frameDB = value;
                OnPropertyChanged();
            }
        }
        public Command DeleteFrame
        {
            get
            {
                return new Command(obj =>
                {
                    using (var db = new warEntities())
                    {
                        FrameDB = db.warframe.OrderBy(x => x.name).Where(x => x.name == SelectedItem.Name).FirstOrDefault();
                        db.Entry(FrameDB).State = EntityState.Deleted;
                        db.warframe.Remove(FrameDB);
                        db.SaveChanges();
                        ISDeleteWF = false;
                    }
                });
            }
        }

        public Command ClearAllDB
        {
            get
            {
                return new Command(obj =>
                {
                    using (var db = new warEntities())
                    {
                        var AllEl = db.element;
                        var AllPolar = db.Polarity;
                        db.element.RemoveRange(AllEl);
                        db.Polarity.RemoveRange(AllPolar);
                        db.SaveChanges();
                    }
                });
            }
        }
        private Command wfDetails;
        public Command WfDetails
        {
            get
            {
                return wfDetails = new Command(obj =>
                {
                    using (var db = new warEntities())
                    {
                        DetailWindow detWindow = new DetailWindow();
                        detWindow.DataContext = this;
                        detWindow.Show();

                        FrameDB = db.warframe.OrderBy(x => x.name).Where(x => x.name == SelectedItem.Name).FirstOrDefault();
                        FrameDB.element = db.element.Where(x => x.id == FrameDB.element_id).FirstOrDefault();
                    }
                });
            }
        }
        private string elementDB;
        public string ElementDB
        {
            get
            {
                return elementDB;
            }
            set
            {
                elementDB = value;
                OnPropertyChanged();
            }
        }
        private List<element> GetElemsList()
        {
            List<element> elemsList = new List<element>();
            using (var db = new warEntities())
            {
                elemsList = db.element.OrderBy(x => x.name).ToList();
            }
            return elemsList;
        }
        public List<string> ElemsList
        {
            get
            {
                return GetElemsList().Select(x => x.name).ToList();
            }
        }
        private ObservableCollection<string> polarDB;
        public ObservableCollection<string> PolarDB
        {
            get
            {
                return polarDB;

            }
            set
            {
                polarDB = value;
                OnPropertyChanged();
            }
        }
        private List<Polarity> GetPolList()
        {
            List <Polarity> polList = new List<Polarity>();
            using (var db = new warEntities())
            {
                polList = db.Polarity.OrderBy(x => x.name).ToList();
            }
            return polList;
        }
        public List<string> PolaritiesList
        {
            get
            {
                return GetPolList().Select(x => x.name).ToList();
            }
        }
        private Polarity selectedPol;
        public string SelectedPol
        {
            get
            {
                if (selectedPol == null)
                    return "";
                else
                    return selectedPol.name;
            }
            set
            {
                selectedPol = GetPolList().Where(x => x.name == value).FirstOrDefault();
                OnPropertyChanged();
            }
        }
        private string selectedPolDB;
        public string SelectedPolDB
        {
            get
            {
                return selectedPolDB;
            }
            set
            {
                selectedPolDB = value;
                OnPropertyChanged();
            }
        }
        public Command EditWf
        {
            get
            {
                return new Command(obj =>
                {
                    try
                    {
                        using (var db = new warEntities())
                        {
                            var newFrame = db.warframe.Find(FrameDB.id);
                            newFrame.element = db.element.Where(x => x.name == ElementDB).FirstOrDefault();
                            newFrame.sex = FrameDB.sex;
                            newFrame.name = FrameDB.name;
                            newFrame.health = FrameDB.health;
                            newFrame.shields = FrameDB.shields;
                            newFrame.armor = FrameDB.armor;
                            newFrame.energy = FrameDB.energy;
                            newFrame.sprint_speed = FrameDB.sprint_speed;

                            var wp = newFrame.warframe_polarity.FirstOrDefault(x => x.polarity_id == SelectedItem.Madurai.polarity_id);
                            if (wp == null)
                                newFrame.warframe_polarity.Add(SelectedItem.Madurai);
                            else if (wp.quantity != SelectedItem.Madurai.quantity)
                                wp.quantity = SelectedItem.Madurai.quantity;
                            wp = newFrame.warframe_polarity.FirstOrDefault(x => x.polarity_id == SelectedItem.Naramon.polarity_id);
                            if (wp == null)
                                newFrame.warframe_polarity.Add(SelectedItem.Naramon);
                            else if (wp.quantity != SelectedItem.Naramon.quantity)
                                wp.quantity = SelectedItem.Naramon.quantity;
                            wp = newFrame.warframe_polarity.FirstOrDefault(x => x.polarity_id == SelectedItem.Vazarin.polarity_id);
                            if (wp == null)
                                newFrame.warframe_polarity.Add(SelectedItem.Vazarin);
                            else if (wp.quantity != SelectedItem.Vazarin.quantity)
                                wp.quantity = SelectedItem.Vazarin.quantity;

                            db.SaveChanges();
                            ISWFDetails = false;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Некорректный ввод!");
                    }
                });
            }
        }
        private bool isParsing;
        public bool IsParsing
        {
            get
            {
                return isParsing;
            }
            set
            {
                isParsing = value; 
                OnPropertyChanged();
            }
        }
        private string progressVis;
        public string ProgressVis
        {
            get
            {
                return progressVis;
            }
            set
            {
                progressVis = value;
                OnPropertyChanged();
            }
        }
        public Command StartParsing
        {
            get
            {
                return new Command(async obj =>
                {
                    ProgressVis = "Visible";
                    IsParsing = false;
                    await Parsing();
                    IsParsing = true;
                    ProgressVis = "Hidden";
                });
            }
        }
        static async Task Parsing()
        {
            await Parser.Parse(@"https://warframe.fandom.com/wiki/WARFRAME_Wiki");
        }
        private Command generateReport;
        public Command GenerateReport
        {
            get
            {
                return generateReport = new Command(obj =>
                {
                    Report.GetInstance().Generate();
                });
            }
        }
    }
}
