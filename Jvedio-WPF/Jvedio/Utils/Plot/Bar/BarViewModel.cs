using Jvedio.Plot.Bar;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Jvedio.Plot.Bar
{
    public class BarViewModel : ViewModelBase
    {
        private double _BarWidth = 70;
        public double BarWidth
        {
            get { return _BarWidth; }
            set
            {
                _BarWidth = value;
                if (CurrentDatas != null)
                {
                    for (int i = 0; i < CurrentDatas.Count; i++)
                    {
                        CurrentDatas[i].BarWidth = value;
                    }
                }
                if (Datas != null)
                {
                    for (int i = 0; i < Datas.Count; i++)
                    {
                        Datas[i].BarWidth = value;
                    }
                }
                RaisePropertyChanged();
            }
        }



        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                RaisePropertyChanged();
            }
        }


        private double _Total=0;
        public double Total
        {
            get { return _Total; }
            set
            {
                _Total = value;
                RaisePropertyChanged();
            }
        }


        private double _Current=0;
        public double Current
        {
            get { return _Current; }
            set
            {
                _Current = value;
                RaisePropertyChanged();
            }
        }

        private bool _Descending = true;
        public bool Descending
        {
            get { return _Descending; }
            set
            {
                _Descending = value;
                RaisePropertyChanged();
            }
        }

        private double Height { get; set; }
        private double Width { get; set; }


        public ObservableCollection<BarData> Datas { get; set; }

        public ObservableCollection<BarData> CurrentDatas { get; set; }


        public BarViewModel(string title,double height, double width, bool descending = true)
        {
            this.Title = title;
            this.Height = height;
            this.Width = width;
            this.Descending = descending;
        }



        public void Init(IEnumerable<BarData> datas,double current=0)
        {
            Current = Math.Floor(this.Width / (BarWidth + 10));
            if (current != 0) Current = current;
            Total = datas.Count();
            Datas = new ObservableCollection<BarData>();
            if (Descending)
                datas = datas.OrderByDescending(o => o.Value).ToList();
            else
                datas = datas.OrderBy(o => o.Value).ToList();
            foreach (var item in datas)
            {
                Datas.Add(item);
            }
            ShowCurrent(Current);
        }




        public void ShowCurrent(double current)
        {
            //等比例换算
            Current = Math.Min(current, Datas.Count);
            int idx = 0;
            if (!Descending) idx = (int)Current - 1;
            CurrentDatas = new ObservableCollection<BarData>();
            if (Datas.Count == 0) return;
            Datas[idx] = new BarData()
            {
                BarWidth = BarWidth,
                Key = Datas[idx].Key,
                Value = Datas[idx].Value,
                ActualValue = Height - 100
            };

            for (int i = 0; i < Current; i++)
            {
                Datas[i] = new BarData()
                {
                    BarWidth = BarWidth,
                    Key = Datas[i].Key,
                    Value = Datas[i].Value,
                    ActualValue = Datas[i].Value / Datas[idx].Value * Datas[idx].ActualValue
                };
            }

            
            for (int i = 0; i < Current; i++)
            {
                CurrentDatas.Add(Datas[i]);
            }
        }
    }
}