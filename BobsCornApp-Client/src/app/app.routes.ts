import { Routes } from "@angular/router";
import { CornPageComponent } from "./modules/corn/components/corn-page.component";
import { MainLayoutComponent } from "./shared/components/main-layout.component";

export const routes: Routes = [
  {
    path: "",
    component: MainLayoutComponent,
    children: [
      {
        path: "",
        component: CornPageComponent,
      },
    ],
  },
];
