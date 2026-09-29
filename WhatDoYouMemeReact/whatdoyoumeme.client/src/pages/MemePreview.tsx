import { useEffect, useState } from "react";
import type { MemeStyle } from "../types/game";

interface MemePreviewProps {
    imageUrl: string;
    caption: string;
    memeStyle: MemeStyle;
}

export default function MemePreview({
    imageUrl,
    caption,
    memeStyle,
}: MemePreviewProps) {
    const [loadedImageUrl, setLoadedImageUrl] = useState<string | null>(null);
    const [imageError, setImageError] = useState(false);

    useEffect(() => {
        let objectUrl: string | null = null;

        async function loadImage() {
            const accessToken = sessionStorage.getItem("accessToken");

            if (!accessToken) {
                setImageError(true);
                return;
            }

            try {
                setImageError(false);

                const response = await fetch(imageUrl, {
                    headers: {
                        "X-Player-Token": accessToken,
                    },
                });

                if (!response.ok) {
                    throw new Error(`Image request failed: ${response.status}`);
                }

                const blob = await response.blob();

                objectUrl = URL.createObjectURL(blob);
                setLoadedImageUrl(objectUrl);
            } catch (error) {
                console.error("Failed to load image:", error);
                setImageError(true);
            }
        }

        loadImage();

        return () => {
            if (objectUrl) {
                URL.revokeObjectURL(objectUrl);
            }
        };
    }, [imageUrl]);

    if (imageError) {
        return (
            <div className="rounded-lg bg-slate-200 p-6 text-center text-slate-600">
                Failed to load image.
            </div>
        );
    }

    if (!loadedImageUrl) {
        return (
            <div className="rounded-lg bg-slate-200 p-6 text-center text-slate-600">
                Loading image...
            </div>
        );
    }

    if (memeStyle === "Classic") {
        return (
            <div className="overflow-hidden rounded-lg bg-black">
                {/* Top black bar */}
                <div className="h-12 bg-black" />

                {/* Image */}
                <div className="flex justify-center bg-black">
                    <img
                        src={loadedImageUrl}
                        alt="meme"
                        className="max-h-[500px] w-full object-contain"
                    />
                </div>

                {/* Bottom black bar / caption */}
                <div className="flex min-h-[140px] items-center justify-center bg-black px-6 py-6">
                    <p className="text-center text-3xl font-extrabold uppercase tracking-wide text-white md:text-4xl">
                        {caption}
                    </p>
                </div>
            </div>
        );
    }

    if (memeStyle === "Impact") {
        return (
            <div className="relative overflow-hidden rounded-lg bg-black">
                <img
                    src={loadedImageUrl}
                    alt="meme"
                    className="max-h-[500px] w-full object-contain"
                />

                <p className="absolute inset-x-0 top-4 px-4 text-center text-3xl font-black uppercase text-white [text-shadow:2px_2px_0_#000,-2px_2px_0_#000,2px_-2px_0_#000,-2px_-2px_0_#000] md:text-4xl">
                    {caption}
                </p>
            </div>
        );
    }

    if (memeStyle === "BottomCaption") {
        return (
            <div className="overflow-hidden rounded-lg bg-white">
                <img
                    src={loadedImageUrl}
                    alt="meme"
                    className="max-h-[500px] w-full object-contain bg-black"
                />

                <div className="px-10 py-5">
                    <p className="text-center text-2xl font-semibold text-slate-800">
                        {caption}
                    </p>
                </div>
            </div>
        );
    }

    return null;
}