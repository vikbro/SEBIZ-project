
export interface Game {
   id: string;
   name: string;
   description: string;
   price: number;
   genre: string;
   developer: string;
   releaseDate: Date;
   tags: string[];
   createdById: string;
   imagePath?: string;
}

export interface User {
    id: string;
    username: string;
    ownedGamesIds: string[];
    balance: number;
}

export interface GameUsage {
    gameId: string;
    playTimeMinutes: number;
    lastPlayed: Date;
    gameTitle: string;
}
