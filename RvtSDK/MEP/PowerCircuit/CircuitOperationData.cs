using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerCircuit
{
    public class CircuitOperationData
    {
        UIDocument uiDocument;
        Selection selection;

        ElectricalSystemSet m_electricalSystemSet;
        List<ElectricalSystemItem> m_electricalSystemItems;
        ElectricalSystem m_selectedElectricalSystem;

        Operation m_operation;
        EditOption m_editOption;

        bool m_canCreateCircuit;
        bool m_hasCircuit;
        bool m_hasPanel;

        public CircuitOperationData(ExternalCommandData commandData)
        {
            this.uiDocument = commandData.Application.ActiveUIDocument;
            this.selection = uiDocument.Selection;

            m_electricalSystemSet = new ElectricalSystemSet();
            m_electricalSystemItems = new List<ElectricalSystemItem>();

            CollectConnectorInfo();
            CollectCircuitInfo();
        }

        private void CollectConnectorInfo()
        {
            m_canCreateCircuit = true;

            bool allLightingDevices = true;
            foreach (ElementId elementId in selection.GetElementIds())
            {
                Element element = uiDocument.Document.GetElement(elementId);
                FamilyInstance fi = element as FamilyInstance;
                if (null == fi)
                {
                    m_canCreateCircuit = false;
                    return;
                }

                if (!String.Equals(fi.Category.Name, "Lighting Devices"))
                {
                    allLightingDevices = false;
                }

                if (!VerifyUnusedConnectors(fi))
                {
                    m_canCreateCircuit = false;
                    return;
                }
            }

            if (allLightingDevices)
            {
                m_canCreateCircuit = false;
            }
        }

        private bool VerifyUnusedConnectors(FamilyInstance fi)
        {
            bool hasUnusedElectricalConnector = false;

            try
            {
                MEPModel mepModel = fi.MEPModel;
                if (null == mepModel)
                {
                    return hasUnusedElectricalConnector;
                }

                ConnectorManager cm = mepModel.ConnectorManager;
                ConnectorSet unusedConnectors = cm.UnusedConnectors;
                if (null == unusedConnectors || unusedConnectors.IsEmpty)
                {
                    return hasUnusedElectricalConnector;
                }

                foreach (Connector connector in unusedConnectors)
                {
                    if (connector.Domain == Domain.DomainElectrical)
                    {
                        hasUnusedElectricalConnector = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return hasUnusedElectricalConnector;
            }

            return hasUnusedElectricalConnector;
        }

        private void CollectCircuitInfo()
        {
            bool bInitilzedElectricalSystemSet = false;

            ElectricalSystem tempElectricalSystem = null;
            foreach (ElementId elementId in selection.GetElementIds())
            {
                Element element = uiDocument.Document.GetElement(elementId);
                FamilyInstance fi = element as FamilyInstance;

                MEPModel mepModel;
                ElectricalSystemSet ess;

                if (fi != null && (mepModel = fi.MEPModel) != null)
                {
                    // Get all electrical systems
                    ess = mepModel.ElectricalSystems;
                    if (null == ess)
                    {
                        m_hasCircuit = false;
                        m_hasPanel = false;
                        return;
                    }

                    // Remove systems which are not power circuits
                    foreach (ElectricalSystem es in ess)
                    {
                        if (es.SystemType != ElectricalSystemType.PowerCircuit)
                        {
                            ess.Erase(es);
                        }
                    }

                    if (ess.IsEmpty)
                    {
                        m_hasCircuit = false;
                        m_hasPanel = false;
                        return;
                    }

                    if (!bInitilzedElectricalSystemSet)
                    {
                        m_electricalSystemSet = ess;
                        bInitilzedElectricalSystemSet = true;
                        continue;
                    }
                    else
                    {
                        foreach (ElectricalSystem es in m_electricalSystemSet)
                        {
                            if (!ess.Contains(es))
                            {
                                m_electricalSystemSet.Erase(es);
                            }
                        }

                        if (m_electricalSystemSet.IsEmpty)
                        {
                            m_hasCircuit = false;
                            m_hasPanel = false;
                            return;
                        }
                    }


                }

                else if ((tempElectricalSystem = element as ElectricalSystem) != null)
                {
                    if (tempElectricalSystem.SystemType != ElectricalSystemType.PowerCircuit)
                    {
                        m_hasCircuit = false;
                        m_hasPanel = false;
                        return;
                    }

                    if (!bInitilzedElectricalSystemSet)
                    {
                        m_electricalSystemSet.Insert(tempElectricalSystem);
                        bInitilzedElectricalSystemSet = true;
                        continue;
                    }

                    if (!m_electricalSystemSet.Contains(tempElectricalSystem))
                    {
                        m_hasCircuit = false;
                        m_hasPanel = false;
                        return;
                    }

                    m_electricalSystemSet.Clear();
                    m_electricalSystemSet.Insert(tempElectricalSystem);
                }

                else
                {
                    m_hasCircuit = false;
                    m_hasPanel = false;
                    return;
                }
            }

            if (!m_electricalSystemSet.IsEmpty)
            {
                m_hasCircuit = true;
                if (m_electricalSystemSet.Size == 1)
                {
                    foreach (ElectricalSystem es in m_electricalSystemSet)
                    {
                        m_selectedElectricalSystem = es;
                        break;
                    }
                }

                foreach (ElectricalSystem es in m_electricalSystemSet)
                {
                    if (!String.IsNullOrEmpty(es.PanelName))
                    {
                        m_hasPanel = true;
                        break;
                    }
                }
            }
        }

        public Operation Operation
        {
            get
            {
                return m_operation;
            }

            set
            {
                m_operation = value;
            }
        }

        public EditOption EditOption
        {
            get
            {
                return m_editOption;
            }
            set
            {
                m_editOption = value;
            }
        }

        public bool CanCreateCircuit
        {
            get
            {
                return m_canCreateCircuit;
            }
        }

        public bool HasCircuit
        {
            get
            {
                return m_hasCircuit;
            }
        }

        public bool HasPanel
        {
            get
            {
                return m_hasPanel;
            }
        }

        public ReadOnlyCollection<ElectricalSystemItem> ElectricalSystemItems
        {
            get
            {
                foreach (ElectricalSystem es in m_electricalSystemSet)
                {
                    ElectricalSystemItem esi = new ElectricalSystemItem(es);
                    m_electricalSystemItems.Add(esi);
                }

                return new ReadOnlyCollection<ElectricalSystemItem>(m_electricalSystemItems);
            }
        }

        public int ElectricalSystemCount
        {
            get
            {
                return m_electricalSystemSet.Size;
            }
        }

        //
        // public Method
        //

        public void Operate()
        {
            Transaction transaction = new Transaction(uiDocument.Document, m_operation.ToString());
            transaction.Start();
            switch (m_operation)
            {
                case Operation.CreateCircuit:
                    CreatePowerCircuit();
                    break;
                case Operation.EditCircuit:
                    EditCircuit();
                    break;
                case Operation.SelectPanel:
                    SelectPanel();
                    break;
                case Operation.DisconnectPanel:
                    DisconnectPanel();
                    break;
                default:
                    break;
            }
            transaction.Commit();

            // Select the modified circuit
            if (m_operation != Operation.CreateCircuit)
            {
                SelectCurrentCircuit();
            }
        }

        public void SelectCircuit(int index)
        {
            // Locate ElectricalSystemItem by index
            ElectricalSystemItem esi = m_electricalSystemItems[index] as ElectricalSystemItem;
            ElementId ei = esi.Id;

            // Locate expected electrical system
            m_selectedElectricalSystem = uiDocument.Document.GetElement(ei) as ElectricalSystem;
            // Select the electrical system
            SelectCurrentCircuit();
        }

        public void ShowCircuit(int index)
        {
            ElectricalSystemItem esi = m_electricalSystemItems[index] as ElectricalSystemItem;
            ElementId ei = esi.Id;
            uiDocument.ShowElements(ei);
        }

        //创建
        private void CreatePowerCircuit()
        {
            List<ElementId> selectionElementId = new List<ElementId>();
            ElementSet elements = new ElementSet();
            foreach (ElementId elementId in selection.GetElementIds())
            {
                elements.Insert(uiDocument.Document.GetElement(elementId));
            }

            foreach (Element e in elements)
            {
                selectionElementId.Add(e.Id);
            }

            try
            {
                // Creation
                ElectricalSystem es = ElectricalSystem.Create(uiDocument.Document, selectionElementId, ElectricalSystemType.PowerCircuit);

                // Select the newly created power system
                selection.GetElementIds().Clear();
                selection.GetElementIds().Add(es.Id);
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToCreateCircuit");
            }
        }

        //编辑
        private void EditCircuit()
        {
            switch (m_editOption)
            {
                case EditOption.Add:
                    AddElementToCircuit();
                    break;
                case EditOption.Remove:
                    RemoveElementFromCircuit();
                    break;
                case EditOption.SelectPanel:
                    SelectPanel();
                    break;
                default:
                    break;
            }
        }

        private void RemoveElementFromCircuit()
        {
            if (!CanExcuteOperation())
            {
                return;
            }

            try
            {
                ElementSet es = new ElementSet();
                foreach (ElementId elementId in selection.GetElementIds())
                {
                    es.Insert(uiDocument.Document.GetElement(elementId));
                }
                if (!m_selectedElectricalSystem.AddToCircuit(es))
                {
                    ShowErrorMessage("FailedToAddElement");
                    return;
                }
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToAddElement");
            }
        }

        private void AddElementToCircuit()
        {
            if (!CanExcuteOperation())
            {
                return;
            }

            try
            {
                // Remove the selected element from circuit
                ElementSet es = new ElementSet();
                foreach (ElementId elementId in uiDocument.Selection.GetElementIds())
                {
                    es.Insert(uiDocument.Document.GetElement(elementId));
                }
                m_selectedElectricalSystem.RemoveFromCircuit(es);
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToRemoveElement");
            }
        }

        private bool CanExcuteOperation()
        {
            selection.GetElementIds().Clear();
            // Interact with UI to select a panel
            if (uiDocument.Selection.PickObject(ObjectType.Element) == null)
            {
                return false;
            }

            // Get the selected element
            Element selectedElement = null;
            foreach (ElementId elementId in uiDocument.Selection.GetElementIds())
            {
                Element element = uiDocument.Document.GetElement(elementId);
                selectedElement = element;
            }

            // Get the MEP model of selected element
            MEPModel mepModel = null;
            FamilyInstance fi = selectedElement as FamilyInstance;
            if (null == fi || null == (mepModel = fi.MEPModel))
            {
                ShowErrorMessage("SelectElectricalComponent");
                return false;
            }

            // Check whether the selected element belongs to the circuit
            if (!IsElementBelongsToCircuit(mepModel, m_selectedElectricalSystem))
            {
                ShowErrorMessage("ElementNotInCircuit");
                return false;
            }

            return true;
        }

        private bool IsElementBelongsToCircuit(MEPModel mepModel, ElectricalSystem selectedElectricalSystem)
        {
            ElectricalSystemSet ess = mepModel.ElectricalSystems;
            if (null == ess || !ess.Contains(selectedElectricalSystem))
            {
                return false;
            }

            return true;
        }

        //选择
        private void SelectPanel()
        {
            selection.GetElementIds().Clear();

            // Interact with UI to select a panel
            if (uiDocument.Selection.PickObject(ObjectType.Element) == null)
            {
                return;
            }

            try
            {
                FamilyInstance fi;
                foreach (ElementId elementId in selection.GetElementIds())
                {
                    Element element = uiDocument.Document.GetElement(elementId);
                    fi = element as FamilyInstance;
                    if (fi != null)
                    {
                        m_selectedElectricalSystem.SelectPanel(fi);
                    }
                }
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToSelectPanel");
            }
        }

        //断开
        private void DisconnectPanel()
        {
            try
            {
                m_selectedElectricalSystem.DisconnectPanel();
            }
            catch (Exception)
            {
                ShowErrorMessage("FailedToDisconnectPanel");
            }
        }

        //选择当前电路
        private void SelectCurrentCircuit()
        {
            selection.GetElementIds().Clear();
            selection.GetElementIds().Add(m_selectedElectricalSystem.Id);
        }

        private void ShowErrorMessage(String message)
        {
            TaskDialog.Show("OperationFailed", message, TaskDialogCommonButtons.Ok);
        }
    }

    public enum Operation
    {
        CreateCircuit,
        EditCircuit,
        SelectPanel,
        DisconnectPanel
    }

    public enum EditOption
    {
        Add,
        Remove,
        SelectPanel
    }

}
