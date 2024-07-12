using midelware.Singleton.Logger;
using System.Text;

namespace signalR
{
    public class BannerApp
    {
       public static void GenerateBanner( )
        {

            string banner = @"
 __        __   _    ____             _        _     _   _       _   _  __ _                _                       
 \ \      / /__| |__/ ___|  ___   ___| | _____| |_  | \ | | ___ | |_(_)/ _(_) ___ __ _  ___(_) ___  _ __   ___  ___ 
  \ \ /\ / / _ \ '_ \___ \ / _ \ / __| |/ / _ \ __| |  \| |/ _ \| __| | |_| |/ __/ _` |/ __| |/ _ \| '_ \ / _ \/ __|
   \ V  V /  __/ |_) |__) | (_) | (__|   <  __/ |_  | |\  | (_) | |_| |  _| | (_| (_| | (__| | (_) | | | |  __/\__ \
    \_/\_/ \___|_.__/____/ \___/ \___|_|\_\___|\__| |_| \_|\___/ \__|_|_| |_|\___\__,_|\___|_|\___/|_| |_|\___||___/
                                                                                                                     "
            ;

            //Console.WriteLine( $"{banner}");
            AppLogger.GetInstance().Info(banner);
        }
    }
}
