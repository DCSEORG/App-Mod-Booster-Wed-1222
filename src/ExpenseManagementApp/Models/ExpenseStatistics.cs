namespace ExpenseManagementApp.Models;

public class ExpenseStatistics
{
    public int TotalExpenses { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public decimal AverageExpenseAmount { get; set; }
}
