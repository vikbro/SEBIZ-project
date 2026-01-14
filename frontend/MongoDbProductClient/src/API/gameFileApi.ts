import API from "./api";

export const uploadGameFile = async (file: File): Promise<{ gameFilePath: string; fileName: string }> => {
    const formData = new FormData();
    formData.append("file", file);

    const response = await API.post("/GameFile/upload", formData, {
        headers: {
            "Content-Type": "multipart/form-data",
        },
    });

    return response.data;
};

export const deleteGameFile = async (fileName: string): Promise<void> => {
    await API.delete(`/GameFile/delete?fileName=${fileName}`);
};

export const deleteImageFile = async (fileName: string): Promise<void> => {
    await API.delete(`/GameFile/image-delete?fileName=${fileName}`);
};
