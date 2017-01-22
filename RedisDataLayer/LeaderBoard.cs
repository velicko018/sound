using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RedisDataLayer
{
   public class LeaderBoard
    {
        private string key = "leaderboard";
        public List<Songs> Songs;
        public List<Groups> Groups;
        public List<Albums> Albums;
        public List<Artists> Artists;

        readonly RedisClient redis = new RedisClient(Config.SingleHost);

        public string GetNextId()
        {
            long nextCounterKey = redis.Incr(key);
            return nextCounterKey.ToString();
        }
        public bool Checkid()
        {
            var test = redis.Get<object>(key);
            return (test != null) ? true : false;
        }

        public LeaderBoard()
        {
            this.Songs = new List<Songs>();
            this.Groups = new List<Groups>();
            this.Albums = new List<Albums>();
            this.Artists = new List<Artists>();
            if (!Checkid())
            {
                var redisCounterSetup = redis.As<string>();
                redisCounterSetup.SetEntry(key, "1");
            }
        }

        public string Set(string value, string LeaderName)
        {
            string tmpkey = this.GetNextId();
            redis.Set<string>(LeaderName+"_"+tmpkey, value);

            return tmpkey;
        }

        public string Get(string key)
        {
            string value = redis.Get<string>(key);
            return value;
        }

        public long SetSongClicks(string SongName, string Writer, string Length, byte Number)
        {
            string id = SongName +"_"+ Writer;
            Songs song = new Songs(SongName, Writer, Length, Number);

            redis.PushItemToList("songs" + id, song.ToJsonString());
            redis.PushItemToList("songs", song.ToJsonString());
            if (Songs.All(item => item.SongName != song.SongName) || Songs.All(item => item.Writer != song.Writer))
            {
                    Songs.Add(song);         
            }

            return redis.Incr("clicks" + id +"songs");
        }
        public long SetArtistClicks(string FirstName, string MiddleName, string LastName, string ArtistName)
        {
            string id = ArtistName;
            Artists artist = new Artists(FirstName, MiddleName, LastName, ArtistName);

            redis.PushItemToList("artists" + id, artist.ToJsonString());
            Artists.Add(artist);

            return redis.Incr("clicks" + id + "artists");
        }
        public long SetGroupClicks(string GroupName, string Origin, string Website, byte NumberOfMembers)
        {
            string id = GroupName;
            Groups group = new Groups(GroupName, Origin, Website, NumberOfMembers);

            Groups.Add(group);
            redis.PushItemToList("groups" + id, group.ToJsonString());

            return redis.Incr("clicks" + id + "groups");
        }
        public long SetAlbumClicks(string AlbumName, long NumberOfCopies, string Producer, string Studio)
        {
            string id = AlbumName +"_"+ Producer;
            Albums album = new Albums(AlbumName, NumberOfCopies, Producer, Studio);

            Albums.Add(album);
            redis.PushItemToList("albums" + id, album.ToJsonString());

            return redis.Incr("clicks" + id+ "albums");
        }

        public long GetSongClicks(string key)
        {
            return redis.Get<long>("clicks" + key + "songs");
        }
        public long GetAlbumClicks(string key)
        {
            return redis.Get<long>("clicks" + key + "albums");
        }
        public long GetGroupClicks(string key)
        {
            return redis.Get<long>("clicks" + key + "groups");
        }
        public long GetArtistClicks(string key)
        {
            return redis.Get<long>("clicks" + key + "artists");
        }

        public List<Songs> GetTopSongs(int ofTop)
        {
            Songs tmp = new Songs();

           for (int i=0; i<this.Songs.Count-1; i++)
            {
                for (int j = i + 1; j < Songs.Count; j++)
                {
                    if (GetSongClicks(Songs[i].SongName + "_" + Songs[i].Writer) >
                        GetSongClicks(Songs[j].SongName + "_" + Songs[j].Writer)) continue;
                    tmp = Songs[i];
                    Songs[i] = Songs[j];
                    Songs[j] = tmp;
                }
            }
            return Songs.Take(ofTop).ToList();
        }
        public List<Albums> GetTopAlbums(int ofTop)
        {
            Albums tmp = new Albums();

            for (int i = 0; i < this.Albums.Count - 1; i++)
            {
                for (int j = i + 1; j < Albums.Count; j++)
                {
                    if (GetAlbumClicks(Albums[i].AlbumName + "_" + Albums[i].Producer) >
                        GetAlbumClicks(Albums[j].AlbumName + "_" + Albums[j].Producer)) continue;
                    tmp = Albums[i];
                    Albums[i] = Albums[j];
                    Albums[j] = tmp;
                }
            }
            return Albums.Take(ofTop).ToList();
        }
        public List<Artists> GetTopArtists(int ofTop)
        {
            Artists tmp = new Artists();

            for (int i = 0; i < this.Artists.Count - 1; i++)
            {
                for (int j = i + 1; j < Artists.Count; j++)
                {
                    if (GetArtistClicks(Artists[i].ArtistName) >
                        GetArtistClicks(Artists[j].ArtistName)) continue;
                    tmp = Artists[i];
                    Artists[i] = Artists[j];
                    Artists[j] = tmp;
                }
            }
            return Artists.Take(ofTop).ToList();
        }
        public List<Groups> GetTopGroups(int ofTop)
        {
            Groups tmp = new Groups();

            for (int i = 0; i < this.Groups.Count - 1; i++)
            {
                for (int j = i + 1; j < Groups.Count; j++)
                {
                    if (GetGroupClicks(Groups[i].GroupName) >
                        GetGroupClicks(Groups[j].GroupName)) continue;
                    tmp = Groups[i];
                    Groups[i] = Groups[j];
                    Groups[j] = tmp;
                }
            }
            return Groups.Take(ofTop).ToList();
        }

        public List<Songs> GetRecentSongs(int ofTop)
        {
            List<Songs> listofSongs = new List<Songs>();

            foreach (string jsonVisitorString in redis.GetRangeFromList("songs", 0, ofTop)) // top 
            {
                Songs song = (Songs)JsonSerializer.DeserializeFromString(jsonVisitorString, typeof(Songs));
                listofSongs.Add(song);
            }

            return listofSongs;
        }
        public List<Groups> GetRecentGroups(int ofTop)
        {
            List<Groups> listofGroups = new List<Groups>();

            foreach (string jsonVisitorString in redis.GetRangeFromList("groups", 0, ofTop)) // top 
            {
                Groups group = (Groups)JsonSerializer.DeserializeFromString(jsonVisitorString, typeof(Groups));
                listofGroups.Add(group);
            }

            return listofGroups;
        }
        public List<Albums> GetRecentAlbums(int ofTop)
        {
            List<Albums> listofAlbums = new List<Albums>();

            foreach (string jsonVisitorString in redis.GetRangeFromList("albums", 0, ofTop)) // top 
            {
                Albums Album = (Albums)JsonSerializer.DeserializeFromString(jsonVisitorString, typeof(Albums));
                listofAlbums.Add(Album);
            }

            return listofAlbums;
        }
        public List<Artists> GetRecentArtists(int ofTop)
        {
            List<Artists> listofArtists = new List<Artists>();

            foreach (string jsonVisitorString in redis.GetRangeFromList("Artists", 0, ofTop)) // top 
            {
                Artists artist  = (Artists)JsonSerializer.DeserializeFromString(jsonVisitorString, typeof(Artists));
                listofArtists.Add(artist);
            }

            return listofArtists;
        }
    }
}
