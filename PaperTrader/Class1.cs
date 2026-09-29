namespace ATAS.Indicators.Technical
{
    using ATAS.DataFeedsCore;
    using ATAS.Indicators.Drawing;
    using Avalonia.Input;
    using Avalonia.Styling;
    using OFT.Attributes;
    using OFT.Attributes.Editors;
    using OFT.Rendering;
    using OFT.Rendering.Context;
    using OFT.Rendering.Control;
    using OFT.Rendering.Tools;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Drawing;
    using System.Windows.Controls;
    using System.Windows.Markup.Localizer;
    using System.Windows.Navigation;
    using Utils.Common;
    using Color = System.Drawing.Color;

    [DisplayName("Paper Trading Simulator")]
    [Category("Trading / Simulation")]
    public class PaperTradingSimulator : Indicator
    {
        /*
         FALTAN LAS FUNCIONES PARA ORDENES LIMITE
         
         */
        public enum positionType
        {
            none,
            mbuy,
            msell,
            lbuy,
            lsell,
            quick
        }
        public struct ConvertedTypes
        {
            public string Type;
            public Color Color;
            public ConvertedTypes(string Type, Color Color)
            {
                this.Type = Type;
                this.Color = Color;
            }
        }
        public struct positions
        {
            public decimal EntryPrice;
            public int laverage;
            public positionType type;
            public decimal EntryMargin;
            public int bar;
            public decimal? pnl;
            //public int? stop;
            //public int? take;
            public positions()
            {
                this.EntryPrice = -1;
                this.laverage = 0;
                this.type = positionType.none;
                this.EntryMargin = 0;
                this.bar = 0;
                //this.stop = stop;
                //this.take = take;
                this.pnl = 0;
            }
            public positions(decimal EntryPrice, int laverage, positionType type,decimal EntryMargin, int bar /*int? stop = null, int? take = null*/)
            {
                this.EntryPrice = EntryPrice;
                this.laverage = laverage;
                this.type = type;
                this.EntryMargin = EntryMargin;
                this.bar = bar;
               //this.stop = stop;
               //this.take = take;
                this.pnl = 0;
            }
        }

        public ValueDataSeries _up = new ValueDataSeries("up")
        {
            VisualType = VisualMode.UpArrow,
            Color = Color.Green,
            Width = 2,
            ShowCurrentValue = true,
            ShowZeroValue = false,
            IsHidden = false,
        };

        public ValueDataSeries _down = new ValueDataSeries("down")
        {
            VisualType = VisualMode.DownArrow,
            Color = Color.Red,
            Width = 2,
            ShowCurrentValue = true,
            ShowZeroValue = false,
            IsHidden = false,
        };

        public RenderFont _font = new RenderFont("Roboto", 14);
        public decimal _balance = 0;

        public decimal og_balance = 0;
        public decimal _entry_margin = 0;
        public decimal Equity = 0;
        public int _laverage = 1;
        public decimal equity_oper = 0;
        public decimal liquidation = 0;

        //EVENT CONTROL
        public bool doble_click = false;
        public bool shift = false; //CONTROL KEY
        public bool click_event = false;

        public decimal stopl = 0;
        public decimal takep = 0;


        public positionType symbol=positionType.none;
        public List<positions> Positions = new();

        /**
         * template
         
         [Display(Name="", GroupName = "", Order = )]
         [Range(typeof(), "", "")]

         */
        #region Properties
        [Display(Name="Balance", GroupName = "Simulated Account", Order = 1)]
        [Range(typeof(decimal), "0.01", "100000000")]
        public decimal Balance {
            get => _balance;
            set {
                og_balance = value;
                _balance = value;
            }
        }
        
        [Display(Name = "Laverage", GroupName = "Simulated Accound", Order = 3)]
        [Range(typeof(int), "1", "150")]
        public int Laverage
        {
            get => _laverage;
            set { _laverage = value; }
        }
        #endregion
        #region ctor

        public PaperTradingSimulator()
            : base(true)
        {
            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            DrawAbovePrice = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final | DrawingLayouts.Historical);

            DataSeries[0].IsHidden = true;
            DataSeries.Add(_up);
            DataSeries.Add(_down);
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
           
        }
        //CLICKS
        public override bool ProcessMouseDown(RenderControlMouseEventArgs e)
        {
            if(e.Button == RenderControlMouseButtons.Middle && shift) {
                click_event = true;
                return true;
            }
            return base.ProcessMouseDown(e);
        }
        public override bool ProcessMouseUp(RenderControlMouseEventArgs e)
        {
            if (e.Button == RenderControlMouseButtons.Middle)
            {
                click_event = false;
            }
            return base.ProcessMouseDown(e);
        }
        public override bool ProcessMouseDoubleClick(RenderControlMouseEventArgs e)
        {
            if (e.Button == RenderControlMouseButtons.Left) {
                doble_click = true;
            }
            return base.ProcessMouseDoubleClick(e);
        }
        //KEYBOARDS
        public override bool ProcessKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                shift = true;
                switch (e.Key)
                {
                    case Key.W:
                        symbol = positionType.mbuy; 
                        break;
                    case Key.S:
                        symbol = positionType.msell; 
                        break;
                    case Key.A:
                        symbol = positionType.quick;
                        break;
                    case Key.R:
                        Laverage = Math.Min(Laverage+1, 150);
                        break;
                    case Key.E:
                        Laverage = Math.Max(Laverage-1, 1);
                        break;
                    case Key.D:
                        _entry_margin = Math.Max(_entry_margin - (decimal)0.01, 0);
                        break;
                    case Key.F:
                        _entry_margin = Math.Min(_entry_margin + (decimal)0.01, Balance);
                        break;
                    case Key.C:
                        _entry_margin = Math.Min(_entry_margin + Math.Max(Math.Round((_entry_margin * 0.25m), 1), (decimal)2.5), Balance);
                        break;
                    case Key.X:
                        _entry_margin = Math.Max(_entry_margin - Math.Max(Math.Round((_entry_margin * 0.25m), 1), (decimal)2.5), 0);
                        break;
                    case Key.Z:
                        Positions.Clear();
                        Equity = 0;
                        Balance = og_balance;
                        break;
                }
                e.Handled = true;
                return true;
            }
            return base.ProcessKeyDown(e);
        }
        public override bool ProcessKeyUp(KeyEventArgs e)
        {
            if(e.Key==Key.LeftShift)
            {
                shift = false;
            }
            return base.ProcessKeyUp(e);
        }
        protected override void OnCalculate(int bar, decimal value)
        {

        }
        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            var lastbar = LastVisibleBarNumber;
            var right = (int)SubPorcentage(Convert.ToDecimal(ChartInfo.Region.Right), 0.25m);
            var color = Color.DarkGray;
            var bar = CurrentBar;
            var cux_bar = ChartInfo.GetXByBar(bar);
            var cuy_bar = ChartInfo.GetYByPrice(GetCandle(bar - 1).Close);
            var CurrentPrice = GetCandle(bar-1).Close;

            var x = ChartInfo.GetXByBar(LastVisibleBarNumber, false);
            var y = ChartInfo.GetYByPrice(GetCandle(bar-1).Close, false);
            
            //OPEN POSITION
            if (symbol != positionType.none)
            {
               SetPosition(new positions(CurrentPrice, Laverage,symbol,_entry_margin, bar));
                if (symbol == positionType.quick) return;
            }

            var barx = ChartInfo.PriceChartContainer.BarsWidth*0.5m;
            var p_size = ChartInfo.PriceChartContainer.PriceRowHeight;
            var pnl = 0m;
            
            var balance_t = "Balance: " + _balance.ToString();
            var equity_t = "Equity:" + equity_oper.ToString();
            //BALANCEE AND LAVERAGE
            var laverage_t = Laverage.ToString() + "x";
            var laverage_text = context.MeasureString(laverage_t, _font);
            var balance_text = context.MeasureString(balance_t, _font);

            var height_diff = balance_text.Height;
            var container_x = Container.Region.Right;
            var container_y = Container.Region.Top;

            var bal_rect = new Rectangle(container_x - (balance_text.Width*2), container_y, balance_text.Width, balance_text.Height);
            var laverage_rect = bal_rect;
            laverage_rect.X = laverage_rect.X - laverage_text.Width;
            laverage_rect.Width = laverage_text.Width;
            laverage_rect.Height = laverage_text.Height;

            DrawBGString(context, Color.Black, Color.White, bal_rect, balance_t);

            DrawBGString(context, Color.Black, Color.White, laverage_rect, laverage_t);

            //EQUITYYY AND MARGIN
            var margin_t = "Margin: " + _entry_margin;

            var margin_text = context.MeasureString(margin_t, _font);
            balance_text = context.MeasureString(equity_t, _font);
            
            var equ_rect = new Rectangle(bal_rect.X, container_y + height_diff, balance_text.Width, balance_text.Height);
            var margin_rect = equ_rect;
            margin_rect.X = margin_rect.X - margin_text.Width;
            margin_rect.Width = margin_text.Width;
            margin_rect.Height = margin_text.Height;

            height_diff += balance_text.Height;


            DrawBGString(context, Color.Black, Color.White, equ_rect, equity_t);

            DrawBGString(context, Color.Black, Color.White, margin_rect, margin_t);

            //TRADES
            for (var i = 0; i < Positions.Count;i++)
            {
                var pos = Positions[i];

                var entry_x = ChartInfo.GetXByBar(pos.bar);
                var entry_y = ChartInfo.GetYByPrice(pos.EntryPrice, false);
                // POSITION LINE AND CANDLE INDICATOR
                var rect = new Rectangle(entry_x - (int)(barx * 1.5m), entry_y - ((int)p_size / 2), (int)barx, (int)p_size);
                var conver = ConvertFromType(pos.type);
                
                context.DrawFillRectangle(new RenderPen(conver.Color), conver.Color, rect);
                context.DrawLine(new RenderPen(conver.Color, Math.Min((float)p_size, 50)), cux_bar, entry_y, container_x, entry_y);

                // PNL CALCULTION
                var singular_pnl = Math.Round(CalculatePnl(pos.EntryPrice, CurrentPrice, pos.EntryMargin,pos.type,pos.laverage),4);
                pos.pnl = singular_pnl;
                Positions[i] = pos;
                pnl = pnl + singular_pnl;

                // SHOW OPEN POSITIONS
                var individual_pnl = conver.Type + " " + pos.EntryPrice + " PnL: " + singular_pnl.ToString();
                var string_size = context.MeasureString(individual_pnl, _font);
                var table_rect = new Rectangle(container_x - string_size.Width, container_y+height_diff, string_size.Width, string_size.Height);

                height_diff += string_size.Height;
                context.DrawString(individual_pnl, _font, conver.Color, table_rect);
            }
            var firstone = Positions.FirstOrDefault(new positions());
            var lastone = Positions.LastOrDefault(new positions());

            var conver_pnl = pnl < 0 ? Color.Red : Color.Green;
            var fpnl_t = pnl.ToString();
            if (lastone.EntryPrice > 0)
            {
                fpnl_t = "PnL: "+fpnl_t + " RoI: " + Math.Round(calculateRoI(pnl, lastone.EntryMargin),2)+"%";
            }

            var mpnl = context.MeasureString(fpnl_t,_font);
            var pnl_rect = new Rectangle(right, y - mpnl.Height / 2, mpnl.Width, mpnl.Height);
            DrawBGString(context, Color.Black, conver_pnl, pnl_rect, fpnl_t);
            equity_oper = Equity + pnl;


            //TAKE P STOP L EVENT 
            if (firstone.EntryPrice <= 0) { 
                doble_click = false;
                return;
            }
            if (liquidation != 0)
            {
                var last_pos = Positions.Last();
                // liquidacion = apalancamiento;
                /* 
                 * se usa el apalancamiento asi porque si uso _laverage se puede cambiar en plena ejecucion
                 */
                // liquidacion = ABS(precio - precio * ((vPos) + (1 / apalancamiento)));

                var liq_price = (last_pos.EntryPrice)-((last_pos.type==positionType.mbuy?1:-1)*(last_pos.EntryPrice * ((1/liquidation))));
                liq_price = Math.Abs(liq_price);
                if((last_pos.type == positionType.mbuy && stopl<=liq_price) || (last_pos.type == positionType.msell && stopl>=liq_price))
                {
                    stopl = 0;
                }
                if((last_pos.type == positionType.mbuy && CurrentPrice <= liq_price) || (last_pos.type == positionType.msell && CurrentPrice >= liq_price))
                {
                    symbol = positionType.quick;
                }
                SetLimit(context, liq_price, Color.Orange, cux_bar, container_x);
            }
            if (doble_click || click_event)
            {
                decimal mouse_cur = MouseLocationInfo.PriceBelowMouse-ChartInfo.PriceChartContainer.Step;
                if(doble_click && !click_event)
                {
                    if (mouse_cur == stopl)
                    {
                        stopl = 0;
                        doble_click = false;
                        return;
                    }
                    if (mouse_cur == takep)
                    {
                        takep = 0;
                        doble_click = false;
                        return;
                    }
                }
                if ((mouse_cur - CurrentPrice) < 0)
                {
                    switch (firstone.type)
                    {
                        case positionType.mbuy:
                            stopl = mouse_cur;
                            break;//sl
                        case positionType.msell:
                            takep = mouse_cur;
                            break;//tp
                    }
                } else
                {
                    switch (firstone.type)
                    {
                        case positionType.mbuy:
                            takep = mouse_cur;
                            break;//tp
                        case positionType.msell:
                            stopl = mouse_cur;
                            break;//sl
                    }
                }
              
                doble_click = false;
           }
            if (stopl != 0)
            {
                SetLimit(context, stopl, Color.Red, cux_bar, container_x);
            } 
            if (takep != 0) { 
                SetLimit(context, takep, Color.Green, cux_bar, container_x);
            }
            //NO OLVIDAR QUE
            /**
             * se invierte el tipo de condicion en base a su positionType
             */
            if(((takep!=0) && (firstone.type == positionType.mbuy ? (CurrentPrice >= takep) : (CurrentPrice <= takep))) || ((stopl!=0) && (firstone.type == positionType.mbuy ? (CurrentPrice <= stopl) : (CurrentPrice >= stopl))))
            {
                symbol = positionType.quick;
                takep = 0;
                stopl = 0;
            }
            if(Positions.Count <=0)
            {
                takep = 0;
                stopl = 0;
            }
           
        }
        public void DrawBGString(RenderContext context, Color bgcolor, Color ctext, Rectangle rect, string text)
        {
            context.DrawFillRectangle(new RenderPen(bgcolor), bgcolor, rect);
            context.DrawString(text, _font, ctext, rect);
        }
        public void SetLimit(RenderContext context, decimal limit, Color color,int init_bar, int end_chart)
        {
            var lim = ChartInfo.GetYByPrice(limit);
            var size_p = (float)ChartInfo.PriceChartContainer.PriceRowHeight;
            context.DrawLine(new RenderPen(color, size_p), init_bar, lim+((int)size_p/2), end_chart, lim + ((int)size_p / 2));

            string type_t = (color == Color.Green ? "Take Profit" : "Stop Loss");
            type_t = color == Color.Orange ? "LIQUIDATION PRICE!!!" : type_t;
            type_t = type_t+ ": " + Math.Round(limit,4).ToString();


            var last = Positions.Last();
            var pricemove = PriceMove(limit, last.EntryPrice);

            var type_pos = last.type == positionType.mbuy ? 1 : -1;

            type_t = color == Color.Orange ? type_t : type_t+(" %"+(type_pos*(Math.Round(pricemove,2)))+" PnL: "+Math.Round(CalculatePnl(last.EntryPrice,limit, last.EntryMargin, last.type, last.laverage),4));
            var mt = context.MeasureString(type_t, _font);
            DrawBGString(context, Color.Black, Color.White, new Rectangle(init_bar, lim-(((mt.Height)-(int)size_p)/2), mt.Width, mt.Height), type_t);
        }
        public void SetPosition(positions position)
        {
            if(position.EntryMargin <= 0)
            {
                symbol = positionType.none;
                return;
            } 
            bool contin = true;
            for(var i=0;i<Positions.Count; i++)
            {
                var pos = Positions[i];
                //QUICK es un protocolo que desbloquea el acceso a todas las posiciones
                if(position.type==positionType.quick||pos.type!=position.type)
                {
                    var final = pos.pnl + pos.EntryMargin;
                    Equity = (decimal)(Equity-pos.EntryMargin);
                    _balance += (decimal)final;

                    var bar = GetCandle(pos.bar - 1);
                    if (pos.type == positionType.mbuy)
                    {
                        _up[pos.bar - 1] = bar.Low;
                    }
                    else if (pos.type == positionType.msell)
                    {
                        _down[pos.bar - 1] = bar.High;
                    }

                    if (position.type!=positionType.quick)
                    {
                        contin = false;
                        Positions.RemoveAt(i);

                        break;
                    }
                }
            }
            symbol = positionType.none;
            if(!contin)
            {
                return;
            } else if (position.type == positionType.quick)
            {
                stopl = 0;
                takep = 0;
                Positions.Clear(); 
                return;
            }
            var result = _balance - position.EntryMargin;
            if(result < 0) return;
            
            Positions.Add(position);
            _balance = result;
            liquidation = _laverage;
            Equity += position.EntryMargin;

        }
        public ConvertedTypes ConvertFromType(positionType type)
        {
            ConvertedTypes parse = new ConvertedTypes();
            switch (type)
            {
                case positionType.mbuy:
                    parse.Type = "Market buy";
                    parse.Color = Color.Green;
                    break;
                case positionType.msell:
                    parse.Type = "Market sell";
                    parse.Color = Color.Red;
                    break;
            }
            return parse;
        }
        public decimal SubPorcentage(decimal original, decimal porcentage)
        {
            return original - (original * porcentage);
        }
        public decimal PriceMove(decimal to, decimal from)
        {
            return ((to - from) / from)*100;
        }
        public decimal calculateRoI(decimal pnl, decimal entrymargin)
        {
            return (pnl / entrymargin) * 100;
        }
        public decimal CalculatePnl(decimal EntryPrice, decimal CurrentPrice, decimal EntryMargin, positionType type, decimal? Laverage=1)
        {
            //entry*((y-x)/x) = pnl 
            var denial = type==positionType.mbuy ? -1 : 1;
            return (EntryMargin*(decimal)Laverage)*(denial*(PriceMove(EntryPrice, CurrentPrice)/100));
        }
        
       

        #endregion

        #region Public API


        #endregion
    }
}