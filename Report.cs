using System;
using System.Linq;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using warframeParse.Models.Entities;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace warframeParse
{
    class Report
    {
        private static Report instance;
        public static Report GetInstance()
        {
            instance = new Report();
            return instance;
        }

        public void Generate()
        {
            string expath = Environment.CurrentDirectory + @"\excelFile.xlsx";
            Excel.Application excelApp = new Excel.Application();
            Workbook workbook = excelApp.Workbooks.Add();
            Worksheet worksheet = workbook.Sheets.Add();

            warframe[] warframes = GetFrames();
            string[] Name = warframes.Select(x => x.name).ToArray();
            string[] Sex = warframes.Select(x => x.sex).ToArray();
            int[] Health = warframes.Select(x => x.health).ToArray();
            int[] Shields = warframes.Select(x => x.shields).ToArray();
            int[] Armor = warframes.Select(x => x.armor).ToArray();
            int[] Energy = warframes.Select(x => x.energy).ToArray();
            double[] Sprint_speed = warframes.Select(x => x.sprint_speed).ToArray();

            CreateChart(worksheet, Name, Health, "Здоровье", XlChartType.xlColumnClustered);
            CreateChart(worksheet, Name, Shields, "Щиты", XlChartType.xlLineMarkers);
            CreateChart(worksheet, Name, Armor, "Броня", XlChartType.xlBarClustered);
            CreateChart(worksheet, Name, Energy, "Энергия", XlChartType.xlLineMarkers);
            workbook.SaveAs(expath);
            workbook.Close();
            if (excelApp != null)
            {
                excelApp.Quit();
                Marshal.ReleaseComObject(excelApp);
            }


            Word.Application wordApp = new Word.Application();
            object docpath = Environment.CurrentDirectory + @"\Разработка БД.doc";
            Document wDoc = wordApp.Documents.Add(ref docpath, false, WdNewDocumentType.wdNewBlankDocument, true);


            Workbook excelbook = excelApp.Workbooks.Open(expath);
            ChartObjects chartObjects = excelbook.Sheets["Лист2"].ChartObjects();
            foreach (ChartObject item in chartObjects)
            {
                Word.Range range1 = wDoc.Content.Paragraphs.Last.Range;
                item.Copy();
                range1.Paste();
                wDoc.Content.Paragraphs.Add();
                range1 = wDoc.Content.Paragraphs.Last.Range;
            }
            try
            {
                wDoc.SaveAs2(expath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            wordApp.Quit(WdSaveOptions.wdPromptToSaveChanges);
        }

        private warframe[] GetFrames()
        {
            using (var db = new warEntities())
            {
                warframe[] frames = db.warframe.OrderBy(x => x.name).ToArray();
                foreach (warframe wf in frames)
                {
                    frames.Append(wf);
                }
                return frames;
            }
        }
        private static void CreateChart(Worksheet worksheet, string[] xValues, int[] yValues, string chartTitle, XlChartType chartType)
        {
            ChartObjects chartObjects = worksheet.ChartObjects();
            ChartObject chartObject = chartObjects.Add(0, 0, 900, 300);
            Excel.Chart chart = chartObject.Chart;

            chart.ChartType = chartType;
            Excel.Range range = worksheet.Range[$"A1:B{xValues.Length}"];
            range.Value = new object[xValues.Length, 2];
            for (int i = 0; i < xValues.Length; i++)
            {
                range.Cells[i + 1, 1].Value = xValues[i];
                range.Cells[i + 1, 2].Value = yValues[i];
            }
            chart.SetSourceData(range);
            chart.HasTitle = true;
            chart.ChartTitle.Text = chartTitle;
        }
        private static void InsertChartIntoWord(Document wordDoc, string expath)
        {
            Paragraph paragraph = wordDoc.Content.Paragraphs.Add();
            InlineShape inlineShape = paragraph.Range.InlineShapes.AddOLEObject(
                ClassType: "Excel.Chart.12",
                FileName: expath,
                LinkToFile: false,
                DisplayAsIcon: false,
                IconFileName: ""
            );
            paragraph.Range.InsertParagraphAfter();
        }
    }
}
