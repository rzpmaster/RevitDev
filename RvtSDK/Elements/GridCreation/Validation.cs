using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Resources;
using System.Windows.Forms;

namespace GridCreation
{
    class Validation
    {
        // Get the resource contains strings
        static ResourceManager resManager = Properties.Resources.ResourceManager;

        public static bool ValidateNumbers(Control number1Ctrl, Control number2Ctrl)
        {
            if (!ValidateNumber(number1Ctrl) || !ValidateNumber(number2Ctrl))
            {
                return false;
            }

            if (Convert.ToUInt32(number1Ctrl.Text) == 0 && Convert.ToUInt32(number2Ctrl.Text) == 0)
            {
                ShowWarningMessage(resManager.GetString("NumbersCannotBeBothZero"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                number1Ctrl.Focus();
                return false;
            }

            return true;
        }

        public static bool ValidateNumber(Control numberCtrl)
        {
            if (!ValidateNotNull(numberCtrl, "Number"))
            {
                return false;
            }

            try
            {
                uint number = Convert.ToUInt32(numberCtrl.Text);
                if (number > 200)
                {
                    ShowWarningMessage(resManager.GetString("NumberBetween0And200"),
                                       Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                    numberCtrl.Focus();
                    return false;
                }
            }
            catch (OverflowException)
            {
                ShowWarningMessage(resManager.GetString("NumberBetween0And200"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                numberCtrl.Focus();
                return false;
            }
            catch (Exception)
            {
                ShowWarningMessage(resManager.GetString("NumberFormatWrong"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                numberCtrl.Focus();
                return false;
            }

            return true;
        }

        public static bool ValidateLength(Control lengthCtrl, String typeName, bool canBeZero)
        {
            if (!ValidateNotNull(lengthCtrl, typeName))
            {
                return false;
            }

            try
            {
                double length = Convert.ToDouble(lengthCtrl.Text);
                if (length <= 0 && !canBeZero)
                {
                    ShowWarningMessage(resManager.GetString(typeName + "CannotBeNegativeOrZero"),
                                       Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                    lengthCtrl.Focus();
                    return false;
                }
                else if (length < 0 && canBeZero)
                {
                    ShowWarningMessage(resManager.GetString(typeName + "CannotBeNegative"),
                                       Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                    lengthCtrl.Focus();
                    return false;
                }
            }
            catch (Exception)
            {
                ShowWarningMessage(resManager.GetString(typeName + "FormatWrong"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                lengthCtrl.Focus();
                return false;
            }

            return true;
        }

        public static bool ValidateCoord(Control coordCtrl)
        {
            if (!ValidateNotNull(coordCtrl, "Coordinate"))
            {
                return false;
            }

            try
            {
                Convert.ToDouble(coordCtrl.Text);
            }
            catch (Exception)
            {
                ShowWarningMessage(resManager.GetString("CoordinateFormatWrong"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                coordCtrl.Focus();
                return false;
            }

            return true;
        }

        public static bool ValidateDegrees(Control startDegree, Control endDegree)
        {
            if (!ValidateDegree(startDegree) || !ValidateDegree(endDegree))
            {
                return false;
            }

            if (Math.Abs(Convert.ToDouble(startDegree.Text) - Convert.ToDouble(endDegree.Text)) <= Double.Epsilon)
            {
                ShowWarningMessage(resManager.GetString("DegreesAreTooClose"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                startDegree.Focus();
                return false;
            }

            if (Convert.ToDouble(startDegree.Text) >= Convert.ToDouble(endDegree.Text))
            {
                ShowWarningMessage(resManager.GetString("StartDegreeShouldBeLessThanEndDegree"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                startDegree.Focus();
                return false;
            }

            return true;
        }

        public static bool ValidateDegree(Control degreeCtrl)
        {
            if (!ValidateNotNull(degreeCtrl, "Degree"))
            {
                return false;
            }

            try
            {
                double startDegree = Convert.ToDouble(degreeCtrl.Text);
                if (startDegree < 0 || startDegree > 360)
                {
                    ShowWarningMessage(resManager.GetString("DegreeWithin0To360"),
                                       Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                    degreeCtrl.Focus();
                    return false;
                }
            }
            catch (Exception)
            {
                ShowWarningMessage(resManager.GetString("DegreeFormatWrong"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                degreeCtrl.Focus();
                return false;
            }

            return true;
        }

        public static bool ValidateLabel(Control labelCtrl, ArrayList allLabels)
        {
            if (!ValidateNotNull(labelCtrl, "Label"))
            {
                return false;
            }

            String labelToBeValidated = labelCtrl.Text;
            foreach (String label in allLabels)
            {
                if (label == labelToBeValidated)
                {
                    ShowWarningMessage(resManager.GetString("LabelExisted"),
                                       Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                    labelCtrl.Focus();
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateNotNull(Control control, String typeName)
        {
            if (String.IsNullOrEmpty(control.Text.TrimStart(' ').TrimEnd(' ')))
            {
                ShowWarningMessage(resManager.GetString(typeName + "CannotBeNull"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                control.Focus();
                return false;
            }

            return true;
        }


        public static bool ValidateLabels(Control label1Ctrl, Control label2Ctrl)
        {
            if (label1Ctrl.Text.TrimStart(' ').TrimEnd(' ') == label2Ctrl.Text.TrimStart(' ').TrimEnd(' '))
            {
                ShowWarningMessage(resManager.GetString("LabelsCannotBeSame"),
                                   Properties.Resources.ResourceManager.GetString("FailureCaptionInvalidValue"));
                label1Ctrl.Focus();
                return false;
            }

            return true;
        }

        public static void ShowWarningMessage(String message, String caption)
        {
            TaskDialog.Show(caption, message, TaskDialogCommonButtons.Ok);
        }
    }
}
