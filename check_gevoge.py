from pymongo import MongoClient
from bson.objectid import ObjectId

connection_uri = "mongodb+srv://viksotodorov_db_user:56qeELfDwpLC7NWN@cluster0.lxn4ylv.mongodb.net/?appName=Cluster0"
client = MongoClient(connection_uri)
db = client["GameStoreDb"]

users_collection = db["Users"]
user = users_collection.find_one({"Username": "gevoge"})

if user:
    print("=== USER INFO ===")
    print(f"Username: {user.get('Username')}")
    print(f"Email: {user.get('Email')}")
    print(f"OwnedGamesIds: {user.get('OwnedGamesIds')}")
    print(f"Number of owned games: {len(user.get('OwnedGamesIds', []))}")
    
    # Get details of owned games
    if user.get('OwnedGamesIds'):
        game_ids = []
        for gid in user.get('OwnedGamesIds', []):
            if isinstance(gid, str) and len(gid) == 24:
                game_ids.append(ObjectId(gid))
            else:
                game_ids.append(gid)
        
        games_collection = db["Games"]
        owned_games = list(games_collection.find({"_id": {"$in": game_ids}}))
        print(f"\n=== OWNED GAMES ({len(owned_games)}) ===")
        for game in owned_games:
            print(f"  - ID: {game.get('_id')} | Name: {game.get('Name')} | Genre: {game.get('Genre')}")
            
        # Now check recommendations (using NEW algorithm)
        print(f"\n=== ANALYZING RECOMMENDATIONS (NEW ALGORITHM) ===")
        
        # Split comma-separated genres
        genre_count = {}
        for game in owned_games:
            genre_str = game.get('Genre')
            if genre_str:
                genres = [g.strip() for g in genre_str.split(',')]
                for genre in genres:
                    genre_count[genre] = genre_count.get(genre, 0) + 1
        
        print(f"Individual genre breakdown: {genre_count}")
        top_genres_sorted = sorted(genre_count.items(), key=lambda x: x[1], reverse=True)[:3]
        top_genre_list = [g[0] for g in top_genres_sorted]
        print(f"Top 3 genres: {top_genre_list}")
        
        # Find recommended games - games that contain at least one of these genres
        owned_game_ids = [game.get('_id') for game in owned_games]
        all_games = list(games_collection.find({"_id": {"$nin": owned_game_ids}}))
        
        recommended = []
        for game in all_games:
            genre_str = game.get('Genre', '')
            if genre_str:
                game_genres = [g.strip() for g in genre_str.split(',')]
                if any(genre in top_genre_list for genre in game_genres):
                    recommended.append(game)
        
        recommended = recommended[:10]
        
        print(f"\n=== RECOMMENDED GAMES ({len(recommended)}) ===")
        for game in recommended:
            print(f"  - Name: {game.get('Name')} | Genre: {game.get('Genre')}")
else:
    print("User 'gevoge' not found")

client.close()
