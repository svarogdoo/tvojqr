import exampleMenu from "$lib/assets/hero-image.webp";

export const restaurants = {
  restoran: {
    name: "Restoran",
    image: exampleMenu,
    description: "Professional menu design",
  },
};

export type RestaurantSlug = keyof typeof restaurants;
