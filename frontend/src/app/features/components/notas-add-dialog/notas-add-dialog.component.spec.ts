import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotasAddDialogComponent } from './notas-add-dialog.component';

describe('NotasAddDialogComponent', () => {
  let component: NotasAddDialogComponent;
  let fixture: ComponentFixture<NotasAddDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotasAddDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotasAddDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
