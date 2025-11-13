import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotasEditDialogComponent } from './notas-edit-dialog.component';

describe('NotasEditDialogComponent', () => {
  let component: NotasEditDialogComponent;
  let fixture: ComponentFixture<NotasEditDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotasEditDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NotasEditDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
