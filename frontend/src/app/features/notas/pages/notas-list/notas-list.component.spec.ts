import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotasListComponent } from './notas-list.component';

describe('NotasListComponent', () => {
  let component: NotasListComponent;
  let fixture: ComponentFixture<NotasListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotasListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotasListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
