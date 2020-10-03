using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PIGIBIG_PI_UPLOADER
{
    class Program
    {
        private static string dropSite = Properties.Settings.Default.DROP_SITE;
        private static string fileExtension = Properties.Settings.Default.FILE_EXT;
        private static string backup = Path.Combine(dropSite, "backup");
        private static List<string> invFiles = null;

        static void Main(string[] args)
        {
            invFiles = new List<string>();

            invFiles = Directory.GetFiles(dropSite, fileExtension).ToList();

            foreach (var item in invFiles)
            {
                try
                {
                    ProcessFile(item);
                    Thread.Sleep(1000);

                    //Move processed fie to backup for reference
                    if (Directory.Exists(backup))
                        File.Move(item, Path.Combine(backup, Path.GetFileName(item)));
                    else
                    {
                        Directory.CreateDirectory(backup);
                        File.Move(item, Path.Combine(backup, Path.GetFileName(item)));
                    }
                }
                catch (Exception er)
                {
                    var sb = new StringBuilder();
                    sb.Append($"{DateTime.Now.ToString()} -> Failed to upload {Path.GetFileName(item)}  {Environment.NewLine} Reason: {er.Message} {Environment.NewLine}");

                    File.AppendAllText(dropSite, sb.ToString());
                }
            }

        }

        static void ProcessFile(string files)
        {
            try
            {
                List<PigibigInvoice> inv = new List<PigibigInvoice>();
                string[] data = new string[] { };


                foreach (var item in File.ReadAllLines(files))
                {
                    var test = item.Replace("\"", "");

                    data = test.Split(',');

                    inv.Add(new PigibigInvoice
                    {
                        Reference = data[0].ToString(),
                        SowNo = data[1].ToString(),
                        ParityNo = data[2].ToString(),
                        TypeTransaction = data[3].ToString(),
                        BatchNo = data[4].ToString(),
                        IdStock = data[5].ToString(),
                        Description = data[6].ToString(),
                        Category = data[7].ToString(),
                        Qty = data[8].ToString(),
                        UOM = data[9].ToString(),
                        Price = data[10].ToString(),
                        TotalAmount = data[11].ToString(),
                        InvoiceDate = data[12].ToString(),
                        CodeNo = data[13].ToString(),
                        AccountNo = data[14].ToString(),
                        BranchCode = data[15].ToString()
                    });
                }

                if (inv.Count > 0)
                {
                    PigibigInvoice pi = new PigibigInvoice();
                    pi.SaveTransaction(inv);
                }

            }
            catch
            {

                throw;
            }
        }
        
    }
}
