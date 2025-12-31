export const restaurants = {
  restoran: {
    name: "Restoran",
    image: "/menu.jpg",
    description: "Professional menu design",
  },
  // Add more restaurants here as needed
  // example: {
  //   name: "Example Cafe",
  //   image: "/example-menu.jpg",
  //   description: "Example cafe menu"
  // }
};

export type RestaurantSlug = keyof typeof restaurants;
