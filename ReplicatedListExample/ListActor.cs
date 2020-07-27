using Godot;
using System;
using System.Collections.Generic;
using MD;

public class MDListICompareNormal : IComparer<string>
{
    public int Compare(string x, string y)
    {
        return x.CompareTo(y);
    }
}

public class MDListICompareInverse : IComparer<string>
{
    public int Compare(string x, string y)
    {
        return y.CompareTo(x);
    }
}

[MDAutoRegister]
public class ListActor : Node2D
{
    public const string GROUP_ACTORS = "LIST_ACTORS";
    [MDReplicated]
    [MDReplicatedSetting(MDReplicatedCommandReplicator.Settings.OnValueChangedEvent, nameof(OnStringListChanged))]
    [MDReplicatedSetting(MDList.Settings.COMPARATOR, typeof(MDListICompareNormal))] // Valid
    [MDReplicatedSetting(MDList.Settings.COMPARATOR, typeof(MDListICompareNormal))] // Invalid, already in list
    [MDReplicatedSetting(MDList.Settings.COMPARATOR, typeof(ListActor))] // Invalid, does not implement ICompare<string>
    [MDReplicatedSetting(MDList.Settings.COMPARATOR, nameof(MDListICompareNormal))] // Invalid, not a type
    [MDReplicatedSetting(MDList.Settings.COMPARATOR, typeof(MDListICompareInverse))] // Valid
    [MDReplicatedSetting(MDList.Settings.UNSAFE_MODE, true)]
    private MDList<String> ReplicatedStringList;

    [MDBindNode("CanvasLayer/ItemList")]
    private ItemList DisplayList;

    [MDBindNode("CanvasLayer/Controls/GridContainer/TxtAddItem")]
    private TextEdit ItemInput;

    [MDBindNode("CanvasLayer/Controls/ChangeNetworkMaster")]
    private MenuButton PopupMenu;

    private MDGameSession GameSession;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AddToGroup(GROUP_ACTORS);
        GameSession = MDStatics.GetGameSession();

        // setup popup
        if (MDStatics.IsServer())
        {
            GameSession.OnPlayerJoinedEvent += OnPlayerJoinedOrLeft;
            GameSession.OnPlayerLeftEvent += OnPlayerJoinedOrLeft;
            OnPlayerJoinedOrLeft(0);
            PopupMenu.GetPopup().Connect("id_pressed", this, nameof(OnChangeNetworkMasterPressed));
        } 
        else
        {
            PopupMenu.Visible = false;
        }

        if (IsNotAllowedToModify())
        {
            return;
        }
        ReplicatedStringList.Add("Initial List String 01");
        ReplicatedStringList.Add("Duplicate 01");
        ReplicatedStringList.Add("Initial List String 02");
        ReplicatedStringList.Add("Duplicate 01");
        ReplicatedStringList.Add("Duplicate 02");
        ReplicatedStringList.Add("Duplicate 01");
        ReplicatedStringList.Add("Intitial List String 03");
        ReplicatedStringList.Add("Duplicate 02");
        ReplicatedStringList.Add("Duplicate 01");
        RefreshList();
    }

    public override void _ExitTree()
    {
        GameSession.OnPlayerJoinedEvent -= OnPlayerJoinedOrLeft;
        GameSession.OnPlayerLeftEvent -= OnPlayerJoinedOrLeft;
    }

    protected virtual void OnPlayerJoinedOrLeft(int PeerId)
    {
        if (MDStatics.IsNetworkActive() && PeerId == GetNetworkMaster())
        {
            // Reset to server if master left
            this.ChangeNetworkMaster(MDStatics.GetServerId());
        }

        PopupMenu.GetPopup().Clear();
        foreach (int player in GameSession.GetAllPeerIds())
        {
            PopupMenu.GetPopup().AddItem(player.ToString());
        }
    }

    public void OnStringListChanged()
    {
        // Remote list has been changed
        RefreshList();
    }

    private void RefreshList()
    {
        DisplayList.Clear();
        foreach (String s in ReplicatedStringList.GetEnumerator())
        {
            DisplayList.AddItem(s);
        }
    }

    private bool IsNotAllowedToModify()
    {
        return MDStatics.IsNetworkActive() && !IsNetworkMaster();
    }

    private void OnAddItemPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        String value = ItemInput.Text;
        if (value != null && value != "")
        {
            ReplicatedStringList.Add(value);
            ItemInput.Text = "";
            RefreshList();
        }
    }


    private void OnRemoveItemPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }
        int[] selection = DisplayList.GetSelectedItems();
        if (selection.Length > 0)
        {
            ReplicatedStringList.RemoveAt(selection[0]);
            RefreshList();
        }
    }

    private void OnRemoveByValuePressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        int[] selection = DisplayList.GetSelectedItems();
        if (selection.Length > 0)
        {
            String value = DisplayList.GetItemText(selection[0]);
            ReplicatedStringList.Remove(value);
            RefreshList();
        }
    }

    private void OnModifyPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        int[] selection = DisplayList.GetSelectedItems();
        String value = ItemInput.Text;
        if (selection.Length > 0 && value != null && value != "")
        {
            ReplicatedStringList[selection[0]] = value;
            ItemInput.Text = "";
            RefreshList();
        }
    }

    private void _OnInsertPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        int[] selection = DisplayList.GetSelectedItems();
        String value = ItemInput.Text;
        if (selection.Length > 0 && value != null && value != "")
        {
            ReplicatedStringList.Insert(selection[0], value);
            ItemInput.Text = "";
            RefreshList();
        }
    }

    private void OnReversePressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        ReplicatedStringList.Reverse();
        RefreshList();
    }

    private void OnAddRangePressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        String value = ItemInput.Text;
        if (value != null && value != "")
        {
            List<String> RangeAdd = new List<string>();
            for (int i =0; i < 5; i++)
            {
                RangeAdd.Add(value + "0" + i);
            }
            ItemInput.Text = "";
            ReplicatedStringList.AddRange(RangeAdd);
            RefreshList();
        }
    }

    private void OnRemoveAllPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        String value = ItemInput.Text;
        if (value != null && value != "")
        {
            ReplicatedStringList.RemoveAll(val => val.EndsWith(value));
            ItemInput.Text = "";
            RefreshList();
        }
    }


    private void OnInsertRangePressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        String value = ItemInput.Text;
        int[] selection = DisplayList.GetSelectedItems();
        if (selection.Length > 0 && value != null && value != "")
        {
            List<String> RangeAdd = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                RangeAdd.Add(value + "0" + i);
            }
            ItemInput.Text = "";
            ReplicatedStringList.InsertRange(selection[0], RangeAdd);
            RefreshList();
        }
    }

    private void OnSortPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }
        ReplicatedStringList.Sort();
        RefreshList();
    }


    private void OnSort01Pressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }
        int[] selection = DisplayList.GetSelectedItems();
        if (selection.Length > 0)
        {
            ReplicatedStringList.Sort(selection[0], ReplicatedStringList.Count - selection[0], typeof(MDListICompareNormal));
        }
        else
        {
            ReplicatedStringList.Sort(typeof(MDListICompareNormal));
        }
        RefreshList();
    }


    private void OnSort02Pressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }
        int[] selection = DisplayList.GetSelectedItems();
        if (selection.Length > 0)
        {
            ReplicatedStringList.Sort(selection[0], ReplicatedStringList.Count - selection[0], typeof(MDListICompareInverse));
        }
        else
        {
            ReplicatedStringList.Sort(typeof(MDListICompareInverse));
        }
        RefreshList();
    }

    private void OnDoUnsafeThingsPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }

        using (MDDisposableList<KeyValuePair<string, IMDDataConverter>> myList = ReplicatedStringList.GetRawList())
        {
            myList.Clear();
            for (int i = 0; i < 20; i++)
            {
                myList.Add(new KeyValuePair<string, IMDDataConverter>("unsafe value " + i, MDStatics.GetConverterForType(typeof(string))));
            }
        }

        RefreshList();
    }

    private void OnClearPressed()
    {
        if (IsNotAllowedToModify())
        {
            return;
        }
        ReplicatedStringList.Clear();
        RefreshList();
    }

    private void OnChangeNetworkMasterPressed(int index)
    {
        // We don't need to check who does this since only server is allowed
        String value = PopupMenu.GetPopup().GetItemText(index);
        this.ChangeNetworkMaster(value.ToInt());
    }

}