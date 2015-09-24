using System.Security.Cryptography;
using System.Text;

namespace StockAnalyzer.StockSecurity
{
   public class StockToolKit
   {
      static private string machineUID = string.Empty;
      static private string pcUID = string.Empty;
      static private string cpuUID = string.Empty;

      static public string GetMachineUID()
      {
         if (machineUID == string.Empty)
         {
            machineUID = GetHash(GetPC_UID() + GetCPU_UId());
         }
         return machineUID;
      }
      static public string GetPC_UID()
      {
         if (pcUID == string.Empty)
         {
            pcUID = GetIdentifier("Win32_BaseBoard", "Model") + GetIdentifier("Win32_BaseBoard", "Manufacturer") + GetIdentifier("Win32_BaseBoard", "Name") + GetIdentifier("Win32_BaseBoard", "SerialNumber");
         }
         return pcUID;
      }
      static public string GetCPU_UId()
      {
         if (cpuUID == string.Empty)
         {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            cpuUID = GetIdentifier("Win32_Processor", "UniqueId");
            if (cpuUID != "") //If no UniqueID, use ProcessorID
            {
               cpuUID += "-";
            }
            cpuUID += GetIdentifier("Win32_Processor", "ProcessorId");
            cpuUID += "-" + GetIdentifier("Win32_Processor", "Name");
            cpuUID += "-" + GetIdentifier("Win32_Processor", "Manufacturer");
            cpuUID += "-" + GetIdentifier("Win32_Processor", "MaxClockSpeed");
         }
         return cpuUID;
      }

      //Return a hardware identifier
      static private string GetIdentifier(string wmiClass, string wmiProperty)
      {
         string result = string.Empty;
         System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
         System.Management.ManagementObjectCollection moc = mc.GetInstances();
         foreach (System.Management.ManagementObject mo in moc)
         {
            //Only get the first one
            if (result == "")
            {
               if (mo[wmiProperty] != null)
               {
                  result = mo[wmiProperty].ToString();
                  break;
               }
            }
         }
         return result;
      }

      static public string GetHash(string s)
      {
         MD5 sec = new MD5CryptoServiceProvider();
         ASCIIEncoding enc = new ASCIIEncoding();
         byte[] bt = enc.GetBytes(s);
         return GetHexString(sec.ComputeHash(bt));
      }
      static public string GetHexString(byte[] bt)
      {
         string s = string.Empty;
         for (int i = 0; i < bt.Length; i++)
         {
            byte b = bt[i];
            int n, n1, n2;
            n = (int)b;
            n1 = n & 15;
            n2 = (n >> 4) & 15;
            if (n2 > 9) s += ((char)(n2 - 10 + (int)'A')).ToString();
            else s += n2.ToString();
            if (n1 > 9) s += ((char)(n1 - 10 + (int)'A')).ToString();
            else s += n1.ToString();
            if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
         }
         return s;
      }
      static public byte[] FromHexString(string s)
      {
         string data = s.Replace("-", "");
         byte[] res = new byte[data.Length / 2];
         int val1, val2;
         int j = 0;
         for (int i = 0; i < data.Length; i++)
         {
            if (data[i] >= 'A' && data[i] <= 'F') val1 = data[i] - 'A' + 10;
            else val1 = data[i] - '0';
            i++;
            if (data[i] >= 'A' && data[i] <= 'F') val2 = data[i] - 'A' + 10;
            else val2 = data[i] - '0';

            res[j++] = (byte)((val1 << 4) | val2);
         }
         return res;
      }
   }
}
