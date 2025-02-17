using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public partial class FormCreditRecurringDateChoose : FormODBase
{
    private readonly CreditCard _creditCard;

    public DateTime DatePay { get; set; }

    public FormCreditRecurringDateChoose(CreditCard creditCard)
    {
        _creditCard = creditCard;

        InitializeComponent();
    }

    private void FormCreditRecurringDateChoose_Load(object sender, EventArgs e)
    {
        if (FillComboBoxMonthSelect())
        {
            return;
        }

        var dateThisMonth = GetValidPayDate(DateTime.Today);
        var dateLastMonth = GetValidPayDate(DateTime.Today.AddMonths(-1));

        if (PrefC.GetBool(PrefName.RecurringChargesUseTransDate))
        {
            labelLastMonth.Text = "Recurring charge date will be: " + dateLastMonth.ToShortDateString();
            labelThisMonth.Text = "Recurring charge date will be: " + dateThisMonth.ToShortDateString();
        }
        else
        {
            labelLastMonth.Text += " " + dateLastMonth.ToShortDateString();
            labelThisMonth.Text += " " + dateThisMonth.ToShortDateString();
        }

        if (dateThisMonth > DateTime.Now)
        {
            butThisMonth.Enabled = false;
            labelThisMonth.Text = "Cannot make payment for future date: " + dateThisMonth.ToShortDateString();
        }

        if (dateLastMonth < _creditCard.DateStart)
        {
            labelLastMonth.Text = "Cannot make payment before start date: " + _creditCard.DateStart;
            butLastMonth.Enabled = false;
        }

        if (dateThisMonth < _creditCard.DateStart)
        {
            labelThisMonth.Text = "Cannot make payment before start date: " + _creditCard.DateStart;
            butThisMonth.Enabled = false;
        }
    }

    private bool FillComboBoxMonthSelect()
    {
        if (CreditCards.GetFrequencyType(_creditCard.ChargeFrequency) == ChargeFrequencyType.FixedDayOfMonth)
        {
            var daysOfMonth = CreditCards.GetDaysOfMonthForChargeFrequency(_creditCard.ChargeFrequency).Split(',').Select(x => SIn.Int(x)).OrderByDescending(x => x).ToList();
            if (daysOfMonth.Count > 1)
            {
                comboBoxMonthSelect.Items.Clear();
                var dateTimes = new List<DateTime>();

                foreach (var dayOfMonth in daysOfMonth)
                {
                    var monthOffset = 0;
                    if (dayOfMonth > DateTime.Today.Day)
                    {
                        monthOffset = -1;
                    }

                    var thisMonth = GetDateForDayOfMonth(DateTime.Today.AddMonths(monthOffset), dayOfMonth);
                    var lastMonth = GetDateForDayOfMonth(DateTime.Today.AddMonths(monthOffset - 1), dayOfMonth);
                    if (thisMonth >= _creditCard.DateStart)
                    {
                        dateTimes.Add(thisMonth);
                    }

                    if (lastMonth >= _creditCard.DateStart)
                    {
                        dateTimes.Add(lastMonth);
                    }
                }

                dateTimes = dateTimes.OrderByDescending(x => x).ToList();
                foreach (var dateTime in dateTimes)
                {
                    comboBoxMonthSelect.Items.Add(dateTime.ToShortDateString(), dateTime);
                }

                if (comboBoxMonthSelect.Items.Count > 0)
                {
                    comboBoxMonthSelect.SelectedIndex = 0;
                }

                EnableComboBoxMonth();
                return true;
            }
        }

        if (CreditCards.GetFrequencyType(_creditCard.ChargeFrequency) == ChargeFrequencyType.FixedWeekDay)
        {
            var dayOfWeekFrequency = CreditCards.GetDayOfWeekFrequency(_creditCard.ChargeFrequency);
            var dayOfWeek = CreditCards.GetDayOfWeek(_creditCard.ChargeFrequency);

            if (dayOfWeekFrequency is not (DayOfWeekFrequency.Every or DayOfWeekFrequency.EveryOther))
            {
                return false;
            }

            FillComboBoxForWeekDays(dayOfWeek, isEveryOther: dayOfWeekFrequency == DayOfWeekFrequency.EveryOther);

            EnableComboBoxMonth();

            return true;
        }

        return false;
    }

    private DateTime GetValidPayDate(DateTime date)
    {
        var datePay = date;

        if (CreditCards.GetFrequencyType(_creditCard.ChargeFrequency) == ChargeFrequencyType.FixedDayOfMonth)
        {
            var listDaysOfMonth = CreditCards.GetDaysOfMonthForChargeFrequency(_creditCard.ChargeFrequency).Split(',').Select(x => SIn.Int(x)).OrderByDescending(x => x).ToList();
            if (listDaysOfMonth.Count == 1)
            {
                var dayOfMonth = listDaysOfMonth.First();

                datePay = GetDateForDayOfMonth(date, dayOfMonth);
            }
            else
            {
                ShowError("Invalid ChargeFrequency.");

                Close();

                return date;
            }
        }

        if (CreditCards.GetFrequencyType(_creditCard.ChargeFrequency) == ChargeFrequencyType.FixedWeekDay)
        {
            var dayOfWeekFrequency = CreditCards.GetDayOfWeekFrequency(_creditCard.ChargeFrequency);
            if (dayOfWeekFrequency is not (DayOfWeekFrequency.Every or DayOfWeekFrequency.EveryOther))
            {
                datePay = CreditCards.GetNthWeekdayofMonth(date, (int) dayOfWeekFrequency - 1, CreditCards.GetDayOfWeek(_creditCard.ChargeFrequency));
            }
            else
            {
                ShowError("Invalid ChargeFrequency.");

                Close();

                return date;
            }
        }

        return datePay;
    }

    private void FillComboBoxForWeekDays(DayOfWeek dayOfWeek, bool isEveryOther = false)
    {
        comboBoxMonthSelect.Items.Clear();
        var dayOfWeekNow = DateTime.Now.DayOfWeek;
        var daysIncremented = 7;
        if (isEveryOther)
        {
            daysIncremented = 14;
        }

        var dateCharge = DateTime.Today;
        dateCharge = dateCharge.AddDays((int) dayOfWeek - (int) dayOfWeekNow); //Get the most recent day of week 
        if (dayOfWeek > dayOfWeekNow)
        {
            //EX: chargeDayOfWeek is Saturday, today is Thursday
            dateCharge = dateCharge.AddDays(-daysIncremented); //since dateCharge is in the future we need to go back a week (or two)
        }

        var dateLimit = dateCharge.AddMonths(-2);
        while (dateLimit <= dateCharge && _creditCard.DateStart <= dateCharge)
        {
            //add charge dates for the last two months within CreditCard.StartDate limits
            comboBoxMonthSelect.Items.Add(dateCharge.ToShortDateString(), dateCharge);
            dateCharge = dateCharge.AddDays(-daysIncremented);
        }

        if (comboBoxMonthSelect.Items.Count > 0)
        {
            comboBoxMonthSelect.SelectedIndex = 0; //Begin with the most recent date.
        }
    }

    private void EnableComboBoxMonth()
    {
        butLastMonth.Visible = false;
        butThisMonth.Visible = false;

        labelLastMonth.Visible = false;
        labelThisMonth.Visible = false;

        if (comboBoxMonthSelect.Items.Count > 0)
        {
            comboBoxMonthSelect.Visible = true;
            labelMonthSelect.Visible = true;
        }
        else
        {
            butOK.Visible = false;
            labelNoDates.Visible = true;
        }
    }

    private static DateTime GetDateForDayOfMonth(DateTime date, int dayOfMonth)
    {
        DateTime dateTime;
        try
        {
            dateTime = new DateTime(date.Year, date.Month, dayOfMonth);
        }
        catch
        {
            dateTime = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        return dateTime;
    }

    private void butLastMonth_Click(object sender, EventArgs e)
    {
        DatePay = GetValidPayDate(DateTime.Today.AddMonths(-1));

        DialogResult = DialogResult.OK;
    }

    private void butThisMonth_Click(object sender, EventArgs e)
    {
        DatePay = GetValidPayDate(DateTime.Today);

        DialogResult = DialogResult.OK;
    }

    private void butOK_Click(object sender, EventArgs e)
    {
        DatePay = comboBoxMonthSelect.GetSelected<DateTime>();

        DialogResult = DialogResult.OK;
    }
}