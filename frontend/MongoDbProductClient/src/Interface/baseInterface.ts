
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
   fileName?: string;
   gameFilePath?: string;
   gameFileName?: string;
}

export interface User {
    id: string;
    username: string;
    email: string;
    ownedGamesIds: string[];
    balance: number;
    role?: string;
    buyerId: string;
    buyerUsername: string;
    sellerId: string;
    sellerUsername: string;
    gameId: string;
    gameTitle: string;
    amount: number;
    transactionDate: Date;
}
