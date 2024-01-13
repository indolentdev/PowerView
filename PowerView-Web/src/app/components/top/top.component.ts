import { Component, OnInit } from '@angular/core';
import { NGXLogger } from 'ngx-logger';
import { MenuService } from '../../services/menu.service';

@Component({
  selector: 'app-top',
  templateUrl: './top.component.html',
  styleUrls: ['./top.component.css']
})
export class TopComponent implements OnInit {
  dayProfilePages: string[];
  monthProfilePages: string[];
  yearProfilePages: string[];
  decadeProfilePages: string[];

  constructor(private log: NGXLogger, private menuService: MenuService) { }

  ngOnInit() {
    this.menuService.getProfileGraphPageChanges().subscribe(_ => {
      this.log.info("Refreshing menu. Reason: ProfileGraphPagesChanged");
      this.getProfilePages();
    });

    this.getProfilePages();
  }

  private getProfilePages(): void {
    this.menuService.getProfilePageNames("day").subscribe(x => { 
      this.dayProfilePages = x.items.sort();
    });

    this.menuService.getProfilePageNames("month").subscribe(x => { 
      this.monthProfilePages = x.items.sort();
    });

    this.menuService.getProfilePageNames("year").subscribe(x => { 
      this.yearProfilePages = x.items.sort();
    });

    this.menuService.getProfilePageNames("decade").subscribe(x => {
      this.decadeProfilePages = x.items.sort();
    });
  }

}
