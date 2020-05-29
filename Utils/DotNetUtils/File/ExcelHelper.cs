using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetUtils.File
{
    public class ExcelHelper : IDisposable
    {
        string filePath = null; //文件名
        IWorkbook workbook = null;
        FileStream fs = null;
        bool disposed;

        public ExcelHelper(string filePath)
        {
            this.filePath = filePath;
            this.disposed = false;
        }

        /// <summary>
        /// 将excel中的数据导入到 DataTable 字典中
        /// Key：SheetName；  Value：DataTable
        /// </summary>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public List<DataTable> ExcelToDataTable(bool isFirstRowColumn)
        {
            List<DataTable> dataTables = new List<DataTable>();
            if (!fileExists)
            {
                //文件不存在
                throw new InvalidOperationException($"指定路径文件{filePath}不存在");
            }

            ISheet sheet = null;
            int startRow = 0;

            fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            string extension = filePath.Substring(filePath.LastIndexOf(".")).ToString().ToLower();
            if (!(extension == ".xlsx" || extension == ".xls"))
            {
                throw new InvalidOperationException($"指定路径文件{filePath}后缀不是Excel文件");
            }

            //判断excel的版本
            if (extension == ".xlsx")
            {
                workbook = new XSSFWorkbook(fs);
            }
            else
            {
                workbook = new HSSFWorkbook(fs);
            }

            for (int n = 0; n < workbook.NumberOfSheets; n++)
            {
                DataTable data = new DataTable();
                sheet = workbook.GetSheetAt(n);
                data.TableName = sheet.SheetName;

                IRow firstRow = sheet.GetRow(0);
                if (null == firstRow)
                    continue;
                int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                if (isFirstRowColumn)
                {
                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                    {
                        ICell cell = firstRow.GetCell(i);
                        if (cell != null)
                        {
                            string cellValue = GetCellValue(cell);
                            if (cellValue != null)
                            {
                                DataColumn column = new DataColumn(cellValue);
                                data.Columns.Add(column);
                            }
                        }
                    }
                    startRow = sheet.FirstRowNum + 1;
                }
                else
                {
                    startRow = sheet.FirstRowNum;
                }

                //最后一列的标号
                int rowCount = sheet.LastRowNum;
                for (int i = startRow; i <= rowCount; ++i)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null) continue; //没有数据的行默认是null

                    DataRow dataRow = data.NewRow();
                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                    {
                        var cellValue = GetCellValue(row.GetCell(j));
                        if (cellValue != null) //同理，没有数据的单元格都默认是null
                            dataRow[j] = cellValue;
                    }
                    data.Rows.Add(dataRow);
                }
                dataTables.Add(data);
            }
            return dataTables;
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="sheetName">要导入的数据</param>
        /// <param name="data">DataTable的列名是否要导入</param>
        /// <param name="isColumnWritten">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(string sheetName, DataTable data, bool isColumnWritten)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;

            fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (null == workbook)
            {
                if (filePath.IndexOf(".xlsx") > 0)
                    workbook = new XSSFWorkbook();
                else if (filePath.IndexOf(".xls") > 0)
                    workbook = new HSSFWorkbook();
            }

            if (workbook != null)
            {
                sheet = workbook.CreateSheet(sheetName);
            }
            else
            {
                return -1;
            }

            if (isColumnWritten == true) //写入DataTable的列名
            {
                IRow row = sheet.CreateRow(0);
                for (j = 0; j < data.Columns.Count; ++j)
                {
                    row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                }
                count = 1;
            }
            else
            {
                count = 0;
            }

            for (i = 0; i < data.Rows.Count; ++i)
            {
                IRow row = sheet.CreateRow(count);
                for (j = 0; j < data.Columns.Count; ++j)
                {
                    row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                }
                ++count;
            }
            workbook.Write(fs); //写入到excel
            return count;
        }

        public void OpenExcelFile()
        {
            System.Diagnostics.Process.Start(filePath);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                        fs.Close();
                }

                fs = null;
                disposed = true;
            }
        }

        //对单元格进行判断取值
        private static string GetCellValue(ICell cell)
        {
            if (cell == null)
                return string.Empty;
            switch (cell.CellType)
            {
                case CellType.Blank: //空数据类型 这里类型注意一下，不同版本NPOI大小写可能不一样,有的版本是Blank（首字母大写)
                    return string.Empty;
                case CellType.Boolean: //bool类型
                    return cell.BooleanCellValue.ToString();
                case CellType.Error:
                    return cell.ErrorCellValue.ToString();
                case CellType.Numeric: //数字类型
                    if (HSSFDateUtil.IsCellDateFormatted(cell))//日期类型
                    {
                        return cell.DateCellValue.ToString();
                    }
                    else //其它数字
                    {
                        return cell.NumericCellValue.ToString();
                    }
                case CellType.Unknown: //无法识别类型
                default: //默认类型
                    return cell.ToString();//
                case CellType.String: //string 类型
                    return cell.StringCellValue;
                case CellType.Formula: //带公式类型
                    try
                    {
                        HSSFFormulaEvaluator e = new HSSFFormulaEvaluator(cell.Sheet.Workbook);
                        e.EvaluateInCell(cell);
                        return cell.ToString();
                    }
                    catch
                    {
                        return cell.NumericCellValue.ToString();
                    }
            }
        }

        private bool fileExists => System.IO.File.Exists(filePath);
    }
}
