using RESTClient;
using Azure.StorageServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tacticsoft;
using System;
using UnityEngine.SceneManagement;

public class ListDemo : MonoBehaviour, ITableViewDataSource
{
  [Header("Azure Storage Service")]
  [SerializeField]
  private string storageAccount;
  [SerializeField]
  private string accessKey;
  [SerializeField]
  private string container;

  private StorageServiceClient client;
  private BlobService blobService;

  [Header("List Demo")]
  [SerializeField]
  private TableView tableView;
  [SerializeField]
  private ListViewCell tableCell;

  private List<Blob> items;
  public Text label;

  // Use this for initialization
  void Start()
  {
    if (string.IsNullOrEmpty(storageAccount) || string.IsNullOrEmpty(accessKey))
    {
      Log.Text(label, "Storage account and access key are required", "Enter storage account and access key in Unity Editor", Log.Level.Error);
    }

    client = StorageServiceClient.Create(storageAccount, accessKey);
    blobService = client.GetBlobService();

    items = new List<Blob>();
    // set TSTableView delegate
    tableView.dataSource = this;

    ListBlobs();
  }

  // Update is called once per frame
  void Update()
  {

  }

  public void OnRefresh()
  {
    ListBlobs();
  }

  public void ListBlobs()
  {
    Log.Text(label, "Listing blobs");
    StartCoroutine(blobService.ListBlobs(ListBlobsCompleted, container));
  }

  private void ListBlobsCompleted(IRestResponse<BlobResults> response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Failed to get list of blobs", "List blob error: " + response.ErrorMessage, Log.Level.Error);
      return;
    }

    Log.Text(label, "Loaded blobs: " + response.Data.Blobs.Length, "Loaded blobs: " + response.Data.Blobs.Length);
    ReloadTable(response.Data.Blobs);
  }

  private void ReloadTable(Blob[] blobs)
  {
    items.Clear();
    items.AddRange(blobs);
    tableView.ReloadData();
  }

  #region ITableViewDataSource

  public int GetNumberOfRowsForTableView(TableView tableView)
  {
    return items.Count;
  }

  public float GetHeightForRowInTableView(TableView tableView, int row)
  {
    return (tableCell.transform as RectTransform).rect.height;
  }

  public TableViewCell GetCellForRowInTableView(TableView tableView, int row)
  {
    ListViewCell cell = tableView.GetReusableCell(tableCell.reuseIdentifier) as ListViewCell;
    if (cell == null)
    {
      cell = (ListViewCell)GameObject.Instantiate(tableCell);
    }
    Blob item = items[row];
    cell.Name.text = item.Name;
    cell.Delete.name = item.Name; // save reference to button name
    return cell;
  }


  #endregion

  /// <summary>
  /// Handler to get selected row item
  /// </summary>
  public void OnSelectedRow(Button button)
  {
    string blobName = button.name;
    Debug.Log("Delete blob: " + blobName);
    foreach (Blob blob in items)
    {
      if (string.Equals(blob.Name, blobName))
      {
        DeleteBlob(blob);
        return;
      }
    }
    Debug.LogWarning("Unable to resolve blob name in list: " + blobName);
  }

  private void DeleteBlob(Blob item)
  {
    // Remove it from the table view list and delete it
    items.Remove(item);
    tableView.ReloadData();
    Debug.Log("Removing blob: " + item.Name);
    StartCoroutine(blobService.DeleteBlob(DeleteBlobCompleted, container, item.Name));
  }

  private void DeleteBlobCompleted(RestResponse response)
  {
    if (response.IsError)
    {
      Log.Text(label, "Couldn't delete blob" + response.StatusCode, "Couldn't delete blob: " + response.ErrorMessage, Log.Level.Error);
      return;
    }
    Log.Text(label, "Deleted blob", "Deleted blob " + response.StatusCode);
  }

  public void TappedNext()
  {
    SceneManager.LoadScene("TextScene");
  }
}
