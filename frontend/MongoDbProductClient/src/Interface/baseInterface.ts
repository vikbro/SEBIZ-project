
export interface Game {
    id: string;
    name: string;
    description: string;
    price: number;
    genre: string;
    developer: string;
    releaseDate: Date;
    tags: string[];
    coverImageUrl?: string;
}

export interface User {
    id: string;
    username: string;
    ownedGamesIds: string[];
    balance?: number;
}
