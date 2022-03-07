using System.Timers;
using System.Configuration;
using System.ServiceProcess;

namespace Service.StockMarket
{
    public partial class Service1 : ServiceBase
    {
        private Timer Timer { get; set; }
        
        private readonly double _interval;

        public Service1()
        {
            InitializeComponent();

            _interval = double.Parse(ConfigurationManager.AppSettings["interval"]);
        }

        protected override void OnStart(string[] args)
        {
            Timer = new Timer();
            Timer.Interval = 1500;
            Timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            Timer.Enabled = false;
        }

        protected override void OnStop()
        {
            // TODO: Adicionar código para este caso.
        }

        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            Timer.Stop();
            Timer.Enabled = false;

            Execute();

            if (Timer.Interval == 1500) Timer.Interval = _interval;

            Timer.Enabled = true;
            Timer.Start();
        }

        public void Execute()
        {
            var controller = new Controller();
            controller.Run();
        }
    }
}
