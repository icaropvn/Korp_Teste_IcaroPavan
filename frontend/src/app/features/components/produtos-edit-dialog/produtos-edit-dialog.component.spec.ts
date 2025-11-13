import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProdutosEditDialogComponent } from './produtos-edit-dialog.component';

describe('ProdutosEditDialogComponent', () => {
  let component: ProdutosEditDialogComponent;
  let fixture: ComponentFixture<ProdutosEditDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProdutosEditDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProdutosEditDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
