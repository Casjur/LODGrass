//public class GrassDataContainer : LoadableStructContainer<GrassTileData>
//{
//    public GrassDataContainer(string fileName) : base(fileName)
//    {
//    }

//    public GrassDataContainer(string fileName, GrassTileData data) : base(fileName, data)
//    {
//    }

//    //public override IEnumerator LoadDataCoroutine(string folderPath)
//    //{
//    //    if (this.IsLoaded)
//    //        return;



//    //    throw new System.NotImplementedException();
//    //}

//    /// <summary>
//    /// Loads all data the Tile is supposed to store.
//    /// !Note: Can only be called from a monoscript class!
//    /// </summary>
//    public override IEnumerator LoadDataCoroutine(string path)
//    {
//        if(this.IsLoaded)
//            yield return null;

//        ResourceRequest request = Resources.LoadAsync<Texture2D>(path); // Assuming the texture is in the "Resources" folder

//        yield return request;

//        if (request.asset != null && request.asset is Texture2D)
//        {
//            Texture2D texture = (Texture2D)request.asset;

//            // Create the struct with the loaded Texture2D
//            this.Data = new GrassTileData
//            {
//                exampleTexture = texture
//            };

//            this.IsLoaded = true;
//        }
//    }

//    //public IEnumerator LoadTextureFromDisk(string filePath, Action<Texture2D> onComplete)
//    //{
//    //    var uri = Path.Combine(Application.streamingAssetsPath, filePath);
//    //    using (var request = UnityWebRequestTexture.GetTexture(uri))
//    //    {
//    //        yield return request.SendWebRequest();
//    //        if (request.result == UnityWebRequest.Result.Success)
//    //        {
//    //            this.Data
//    //            texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
//    //            onComplete?.Invoke(texture);
//    //        }
//    //        else
//    //        {
//    //            Debug.LogError($"Failed to load texture from disk: {request.error}");
//    //            onComplete?.Invoke(null);
//    //        }
//    //    }
//    //}


//    public override void SaveData(string folderPath)
//    {
//        throw new System.NotImplementedException();
//    }
//}
